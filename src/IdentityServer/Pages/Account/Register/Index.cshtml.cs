using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityModel;
using IdentityServer.Modules.IdentityManagement.UseCases.Passwords.GetPasswordRequirements;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserEmailRequireConfirmation;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserSignedIn;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;
using IdentityServer.Security.Authentication.Model;
using IdentityServer.Security.Authentication.Results;
using IdentityServer.Security.Authentication.Schemes.Abstract;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using IdentityServer.Security.Authorization;
using IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IAuthenticationSchemeService _authenticationSchemeService;
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeService authenticationSchemeService,
        ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher,
        ILogger<RegisterModel> logger)
    {
        _interaction = interaction;
        _authenticationSchemeService = authenticationSchemeService;
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _logger = logger;
    }

    public ViewModel View { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        returnUrl = !string.IsNullOrWhiteSpace(returnUrl) ? returnUrl : Url.Content(PathConstants.RootRelativePath);

        var isUserSignedInQuery = new IsUserSignedInQuery();
        bool userIsSignedIn = await _queryDispatcher.DispatchAsync(isUserSignedInQuery);

        if (userIsSignedIn)
        {
            return Redirect(returnUrl);
        }

        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        AuthenticateResult authenticateResult = await HttpContext.AuthenticateExternalAsync();

        if (authenticateResult.Succeeded)
        {
            // Use the return URL from the authentication result.
            Input.ReturnUrl = authenticateResult.GetReturnUrl(Input.ReturnUrl).ToString();

            ClaimsPrincipal externalUser = authenticateResult.Principal;

            Input.ExternalProvider = authenticateResult.GetExternalProvider();
            Input.ExternalUserId = externalUser.GetExternalUserId();

            Input.Email = externalUser.GetEmailFromClaims();

            Input.FirstName = externalUser.FindFirstValue(JwtClaimTypes.GivenName) ??
                              externalUser.FindFirstValue(ClaimTypes.GivenName);

            Input.LastName = externalUser.FindFirstValue(JwtClaimTypes.FamilyName) ??
                             externalUser.FindFirstValue(ClaimTypes.Surname);
        }

        await BuildViewModelAsync(returnUrl);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.ReturnUrl ??= Url.Content(PathConstants.RootRelativePath);

        // Check if we are in the context of an authorization request.
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // The user clicked the "cancel" button.
        if (Input.Button != "register")
        {
            if (Input.IsExternalLogin)
            {
                // Delete temporary cookie used during external authentication.
                await HttpContext.SignOutExternalAsync();
            }

            if (authorizationRequest == null)
            {
                // Since we don't have a valid context, then we just go back to the home page.
                return Redirect(PathConstants.RootRelativePath);
            }

            // This "can't happen", because if the ReturnUrl was null, then the context would be null.
            ArgumentNullException.ThrowIfNull(Input.ReturnUrl);

            // If the user cancels, send a result back into IdentityServer
            // as if they denied the consent (even if this client does not require consent).
            // This will send back an access denied OIDC error response to the client.
            await _interaction.DenyAuthorizationAsync(authorizationRequest, AuthorizationError.AccessDenied);

            return this.Redirect(authorizationRequest, Input.ReturnUrl, Redirect);
        }

        // TODO: I think we might have to repopulate the view model since it is not bound.
        await BuildViewModelAsync(Input.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: This could call the API endpoint, but it really doesn't matter if we're using the app service layer.
        // This is just an alternative way to execute the command (which should be the same).

        var userId = Guid.NewGuid();

        // TODO: Do we want separate command for external users?

        var registerUserCommand = new RegisterUserCommand(userId.ToString(),
            Input.Email,
            Input.Password,
            Input.FirstName,
            Input.LastName,
            Input.IsExternalLogin,
            Input.ExternalProvider,
            Input.ExternalUserId,
            Input.ReturnUrl
        );

        Result<string> registerUserResult = await _commandDispatcher.DispatchAsync(registerUserCommand, CancellationToken.None);

        if (registerUserResult.Failed)
        {
            foreach (IError error in registerUserResult.Errors)
            {
                // TODO: Map error codes to input fields?
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        var getUserByIdQuery = new GetUserByIdQuery(registerUserResult.Value);
        Result<User> getUserByIdQueryResult = await _queryDispatcher.DispatchAsync(getUserByIdQuery);

        if (getUserByIdQueryResult.Failed)
        {
            foreach (IError error in getUserByIdQueryResult.Errors)
            {
                _logger.LogError("{ErrorCode}: {ErrorMessage}", error.Code, error.Message);
            }

            throw new Exception("One or more errors occurred while retrieving the user after registration.");
        }

        User user = getUserByIdQueryResult.Value;

        // This is used for account activation.
        var doesUserEmailRequireConfirmationQuery = new DoesUserEmailRequireConfirmationQuery(user.UserId);
        bool userEmailRequiresConfirmation = await _queryDispatcher.DispatchAsync(doesUserEmailRequireConfirmationQuery);

        if (userEmailRequiresConfirmation)
        {
            // Delete temporary cookie used during external authentication.
            await HttpContext.SignOutExternalAsync();
            return RedirectToPage(AccountPageConstants.EmailConfirmationSent, new { userId = user.UserId, Input.ReturnUrl });
        }

        if (Input.IsExternalLogin)
        {
            var signInExternalCommand = new SignInExternalCommand(user.UserId, Input.ExternalProvider, Input.ExternalUserId, Input.ReturnUrl);
            Result<ExternalSignIn> signInExternalCommandResult = await _commandDispatcher.DispatchAsync(signInExternalCommand);

            if (signInExternalCommandResult.Succeeded)
            {
                return this.Redirect(authorizationRequest, Input.ReturnUrl, Redirect);
            }

            foreach (IError error in getUserByIdQueryResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        var signInCommand = new SignInCommand(user.UserId, Input.ExternalProvider, Input.ExternalUserId, false, Input.ReturnUrl);
        Result<bool> signInCommandResult = await _commandDispatcher.DispatchAsync(signInCommand);

        if (signInCommandResult.Succeeded)
        {
            return Redirect(Input.ReturnUrl);
        }
        
        foreach (IError error in getUserByIdQueryResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Message);
        }

        return Page();
    }

    private async Task BuildViewModelAsync(string? returnUrl)
    {
        View = new ViewModel
        {
            WelcomeMessage = Input.IsExternalLogin
                ? $"You're just a few steps away from completing your registration with your {Input.ExternalProvider ?? "external"} account. " +
                  $"Please verify and fill out the form below."
                : "Please fill out the form below to create your account."
        };

        if (!Input.IsExternalLogin)
        {
            var getPasswordRequirementsQuery = new GetPasswordRequirementsQuery();
            PasswordRequirements passwordRequirements = await _queryDispatcher.DispatchAsync(getPasswordRequirementsQuery);

            View.PasswordConfig = new PasswordConfiguration
            {
                RequiredLength = passwordRequirements.RequiredLength,
                RequiredUniqueChars = passwordRequirements.RequiredUniqueChars,
                RequireNonAlphanumeric = passwordRequirements.RequireNonAlphanumeric,
                RequireLowercase = passwordRequirements.RequireLowercase,
                RequireUppercase = passwordRequirements.RequireUppercase,
                RequireDigit = passwordRequirements.RequireDigit
            };

            // This contains information about the current authorization request.
            AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(returnUrl);
            List<ExternalProvider> providers = await _authenticationSchemeService.GetProvidersAsync();
            Client? client = authorizationRequest?.Client;
            providers = _authenticationSchemeService.FilterByClient(client, providers);
            View.ExternalProviders = providers;
        }
    }
}
