using Duende.IdentityServer.Models;
using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Security.Authentication.External.Abstract;
using IdentityServer.Security.Authentication.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using ExternalLoginInfo = IdentityServer.Security.Authentication.Model.ExternalLoginInfo;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInExternalCommandHandler : CommandHandler<SignInExternalCommand, Result<ExternalSignIn>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly ILogger<SignInExternalCommandHandler> _logger;

    public SignInExternalCommandHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthenticationSchemeProvider schemeProvider,
        IExternalAuthenticationService externalAuthenticationService,
        ILogger<SignInExternalCommandHandler> logger,
        IEventRaisingFacade eventRaisingFacade) : base(eventRaisingFacade)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _schemeProvider = schemeProvider;
        _externalAuthenticationService = externalAuthenticationService;
        _logger = logger;
    }

    public override async Task<Result<ExternalSignIn>> HandleAsync(SignInExternalCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithId(command.UserId));
        }

        if (string.IsNullOrWhiteSpace(command.ExternalProvider))
        {
            return Error.NotFound("SignInExternal.ExternalProviderRequired", "An external provider is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ExternalUserId))
        {
            return Error.NotFound("SignInExternal.ExternalUserIdRequired", "An external user ID is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ReturnUrl))
        {
            return Error.Validation("SignIn.ReturnUrl", "A valid return URL must be provided.");
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new Exception("Http context is null.");
        }

        AuthenticationScheme authenticationScheme = await _schemeProvider.GetSchemeAsync(command.ExternalProvider) ??
                                                    throw new InvalidOperationException(ErrorMessages.AuthenticationSchemeNotFound);

        var externalLoginInfo = new ExternalLoginInfo(authenticationScheme.Name, command.ExternalUserId, command.ReturnUrl);

        // These will likely have to stay here at the page since the authorization request is used below for completing the sign in.
        AuthenticateResult authenticateResult = await httpContext.AuthenticateExternalAsync();

        if (!authenticateResult.Succeeded)
        {
            throw new InvalidOperationException(ErrorMessages.ExternalAuthenticationError(authenticateResult));
        }

        AuthorizationRequest? authorizationRequest = await _externalAuthenticationService.CompleteExternalLogin(httpContext, externalLoginInfo, authenticateResult, user);
        
        _logger.LogInformation("User with ID \"{UserId}\" signed in using external provider: {ExternalProviderName}.", user.Id, externalLoginInfo.ExternalProvider);

        var externalSignIn = new ExternalSignIn(true, authorizationRequest);

        return externalSignIn;
    }
}