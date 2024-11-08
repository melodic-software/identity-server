using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.ReturnUrls.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserSignedIn;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;
using IdentityServer.Security.Authentication.Model;
using IdentityServer.Security.Authentication.Schemes.Abstract;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using IdentityServer.Security.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Login;

[AllowAnonymous]
public class Index : PageModel
{
    public const string CancelButtonValue = "cancel";
    public const string LoginButtonValue = "login";

    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IAuthenticationSchemeService _authenticationSchemeService;
    private readonly ILogger<Index> _logger;

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(
        ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher,
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IAuthenticationSchemeService authenticationSchemeService,
        ILogger<Index> logger)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _logger = logger;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _authenticationSchemeService = authenticationSchemeService;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        _logger.LogInformation("Login page loaded. Return URL: {ReturnUrl}", returnUrl);

        var isUserSignedInQuery = new IsUserSignedInQuery();
        bool userIsSignedIn = await _queryDispatcher.DispatchAsync(isUserSignedInQuery);

        if (userIsSignedIn)
        {
            return Redirect(!string.IsNullOrWhiteSpace(returnUrl) ? returnUrl : PathConstants.RootRelativePath);
        }

        // Ensure the external provider cookie is cleared (if exists).
        await HttpContext.SignOutExternalAsync();

        // Model is built to show this page.
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            // We only have one option for logging in, and it's an external provider.
            return RedirectToPage(PageConstants.ExternalLoginChallenge, new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // This is where the username and password combination is checked.
        // External logins are handled separately.

        // Check if we are in the context of an authorization request.
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // The user clicked the "cancel" button.
        if (Input.Button != LoginButtonValue)
        {
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

            Input.ReturnUrl ??= PathConstants.RootRelativePath;
            return this.Redirect(authorizationRequest, Input.ReturnUrl, Redirect);
        }

        if (ModelState.IsValid)
        {
            var getUserByEmailQuery = new GetUserByEmailQuery(Input.Email);
            Result<User> queryResult = await _queryDispatcher.DispatchAsync(getUserByEmailQuery);

            if (queryResult.Failed)
            {
                if (queryResult.Errors.ContainsNotFound())
                {
                    ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
                    await BuildModelAsync(Input.ReturnUrl);
                }
                else
                {
                    foreach (IError error in queryResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Message);
                    }

                    await BuildModelAsync(Input.ReturnUrl);
                }

                return Page();
            }

            User user = queryResult.Value;

            var signInLocalCommand = new SignInLocalCommand(
                userName: null,
                email: Input.Email,
                Input.Password,
                Input.RememberLogin,
                Input.ReturnUrl,
                Input.DeviceId
            );

            Result<SignInLocalResult> commandResult = await _commandDispatcher.DispatchAsync(signInLocalCommand);

            if (commandResult.Failed)
            {
                // We can't sign them in yet if their email has not been confirmed.
                if (commandResult.Errors.Any(x => x.Code == UserErrorCodes.RequiresConfirmedEmail))
                {
                    return RedirectToPage(AccountPageConstants.ConfirmEmail, new { userId = user.UserId, Input.ReturnUrl });
                }

                foreach (IError error in commandResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Message);
                }

                await BuildModelAsync(Input.ReturnUrl);

                return Page();
            }

            SignInLocalResult signInLocalResult = commandResult.Value;

            if (!string.IsNullOrWhiteSpace(signInLocalResult.MfaRedirectUri))
            {
                return Redirect(signInLocalResult.MfaRedirectUri);
            }

            if (authorizationRequest != null)
            {
                // This "can't happen", because if the ReturnUrl was null, then the context would be null.
                ArgumentNullException.ThrowIfNull(Input.ReturnUrl);

                // We can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null.
                Input.ReturnUrl ??= PathConstants.RootRelativePath;
                return this.Redirect(authorizationRequest, Input.ReturnUrl, Redirect);
            }

            // Request for a local page.
            if (Url.IsLocalUrl(Input.ReturnUrl))
            {
                return Redirect(Input.ReturnUrl);
            }

            if (string.IsNullOrEmpty(Input.ReturnUrl))
            {
                return Redirect(PathConstants.RootRelativePath);
            }

            _logger.LogInvalidReturnUrl(Input.ReturnUrl);

            throw new ArgumentException(ErrorMessages.InvalidReturnUrl);
        }

        // Something went wrong... show form with error.
        await BuildModelAsync(Input.ReturnUrl);

        return Page();
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        // This contains information about the current authorization request.
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(returnUrl);

        // There is no local login screen in this scenario.
        // This is a shortcut in case we're using IdentityServer as a federation gateway to another external identity provider.
        if (authorizationRequest?.IdP != null && await _schemeProvider.GetSchemeAsync(authorizationRequest.IdP) != null)
        {
            bool local = authorizationRequest.IdP == IdentityServerConstants.LocalIdentityProvider;

            // This is meant to short circuit the UI and only trigger the one external IdP.
            View = new ViewModel
            {
                EnableLocalLogin = local,
            };

            Input.Email = authorizationRequest.LoginHint ?? throw new Exception(ErrorMessages.InvalidLoginHint);

            if (local)
            {
                return;
            }

            View.ExternalProviders =
            [
                new ExternalProvider(authenticationScheme: authorizationRequest.IdP)
            ];

            return;
        }

        List<ExternalProvider> providers = await _authenticationSchemeService.GetProvidersAsync();

        // Does the client allow local login?
        bool allowLocal = true;
        Client? client = authorizationRequest?.Client;
        allowLocal = client?.EnableLocalLogin ?? allowLocal;

        providers = _authenticationSchemeService.FilterByClient(client, providers);

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            LoginCallbackUri = Url.PageLink(AccountPageConstants.LoginCallback),
            LoginButtonValue = LoginButtonValue,
            CancelButtonValue = CancelButtonValue,
            ExternalProviders = [.. providers]
        };
    }
}
