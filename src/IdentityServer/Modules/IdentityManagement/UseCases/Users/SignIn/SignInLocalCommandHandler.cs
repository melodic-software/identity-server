using Authsignal;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Pages.Account;
using IdentityServer.Security.Authentication.SignIn;
using IdentityServer.Security.Mfa.AuthSignal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Telemetry = IdentityServer.Diagnostics.Telemetry;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInLocalCommandHandler : CommandHandler<SignInLocalCommand, Result<SignInLocalResult>>
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly AuthsignalTrackingService _authsignalTrackingService;
    private readonly IUrlHelper _urlHelper;

    public SignInLocalCommandHandler(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IEventService events,
        AuthsignalTrackingService authsignalTrackingService,
        IUrlHelper urlHelper,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _events = events;
        _authsignalTrackingService = authsignalTrackingService;
        _urlHelper = urlHelper;
    }

    public override async Task<Result<SignInLocalResult>> HandleAsync(SignInLocalCommand command, CancellationToken cancellationToken = default)
    {
        // Check if we are in the context of an authorization request.
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(command.ReturnUrl);

        ApplicationUser? user = await FindUserAsync(command);

        if (user == null)
        {
            await RecordLoginFailureAsync(authorizationRequest, user);
            return UserErrors.NotFound;
        }

        // If configured, users must have confirmed their email before they can be allowed to sign in.
        if (_userManager.Options.SignIn.RequireConfirmedAccount && !user.EmailConfirmed && _userManager.SupportsUserEmail)
        {
            return UserErrors.RequiresConfirmedEmail;
        }

        // If we want to support multiple different external MFA providers, this will have to change.
        // If we have dynamic (multiple) providers, then a factory is likely needed.
        // This should be fairly simple though, and maybe we can just refactor down the line if needed.
        bool authSignalEnabled = _configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false);

        SignInResult result = await GetSignInResultAsync(command, authSignalEnabled, user);

        if (result.IsNotAllowed)
        {
            await RecordLoginFailureAsync(authorizationRequest, user);
            return UserErrors.IsNotAllowedToSignIn;
        }

        if (result.IsLockedOut)
        {
            await RecordLoginFailureAsync(authorizationRequest, user);
            return UserErrors.IsLockedOut;
        }

        if (authSignalEnabled)
        {
            string? encodedReturnUrl = !string.IsNullOrWhiteSpace(command.ReturnUrl)
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(command.ReturnUrl))
                : null;

            string authsignalRedirectUrl = _urlHelper.PageLink(AccountPageConstants.LoginCallback, values: new
            {
                returnUrl = encodedReturnUrl,
                rememberLogin = command.RememberLogin,
            });

            var customData = new Dictionary<string, string>()
            {
                { "EmailConfirmed", user.EmailConfirmed.ToString() },
                { "PhoneNumberConfirmed", user.PhoneNumberConfirmed.ToString() },
                { "AccessFailedCount", user.AccessFailedCount.ToString(CultureInfo.InvariantCulture) }
            };

            TrackResponse trackResponse = await _authsignalTrackingService.GetTrackResponseAsync(
                "identity-server-login",
                authsignalRedirectUrl,
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                command.DeviceId,
                customData,
                cancellationToken
            );

            // Right now we only prompt if they have enrolled.
            if (trackResponse is { IsEnrolled: true, State: UserActionState.CHALLENGE_REQUIRED })
            {
                return new SignInLocalResult(trackResponse.Url);
            }
        }
        else if (result.RequiresTwoFactor)
        {
            string? pageLink = _urlHelper.PageLink(AccountPageConstants.LoginWith2fa, values: new
            {
                returnUrl = command.ReturnUrl,
                rememberMe = command.RememberLogin
            });

            return new SignInLocalResult(pageLink);
        }

        if (result.Succeeded)
        {
            // If MFA is not required, we can immediately sign them in since we use the check sign in method with Authsignal enabled.
            // All previous account checks should have been made by this point.
            await _signInManager.SignInAsync(user, isPersistent: command.RememberLogin);
            await SignInSuccessService.HandleSuccessfulSignInAsync(user, _userManager, authorizationRequest, _events);

            return new SignInLocalResult(null);
        }

        await RecordLoginFailureAsync(authorizationRequest, user);
        return UserErrors.InvalidCredentials;
    }

    private async Task<ApplicationUser?> FindUserAsync(SignInLocalCommand command)
    {
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            user = await _userManager.FindByEmailAsync(command.Email);
        }

        if (user == null && !string.IsNullOrWhiteSpace(command.UserName))
        {
            user = await _userManager.FindByNameAsync(command.UserName);
        }

        return user;
    }

    private async Task<SignInResult> GetSignInResultAsync(SignInLocalCommand command, bool authSignalEnabled, ApplicationUser user)
    {
        SignInResult result;

        if (authSignalEnabled)
        {
            // We're relying on an external MFA provider. We can simply validate the users credentials here.
            result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        }
        else
        {
            // Otherwise we have to attempt a full login in order to evaluate MFA status with ASP.NET Identity.
            result = await _signInManager.PasswordSignInAsync(user, command.Password, lockoutOnFailure: true, isPersistent: command.RememberLogin);
        }

        return result;
    }

    private async Task RecordLoginFailureAsync(AuthorizationRequest? authorizationRequest, ApplicationUser? user)
    {
        var loginFailureEvent = new UserLoginFailureEvent(
            user?.UserName ?? user?.Email,
            "invalid credentials",
            clientId: authorizationRequest?.Client.ClientId
        );

        await _events.RaiseAsync(loginFailureEvent);

        Telemetry.Metrics.UserLoginFailure(
            authorizationRequest?.Client.ClientId,
            IdentityServerConstants.LocalIdentityProvider,
            "invalid credentials"
        );
    }
}
