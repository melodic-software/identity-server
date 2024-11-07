using Duende.IdentityServer.Models;
using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Constants;
using IdentityServer.Logging;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserEmailRequireConfirmation;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByExternalLogin;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserLockedOut;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;
using IdentityServer.Pages.Account;
using IdentityServer.Security.Authentication.Results;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using IdentityServer.Security.Authorization;
using IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServer.Pages.ExternalLogin;

[AllowAnonymous]
public class Callback : PageModel
{
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly ILogger<Callback> _logger;

    public Callback(
        IQueryDispatchFacade queryDispatcher,
        ICommandDispatchFacade commandDispatcher,
        ILogger<Callback> logger)
    {
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }
        
    public async Task<IActionResult> OnGet()
    {
        try
        {
            AuthenticateResult authenticateResult = await HttpContext.AuthenticateExternalAsync();

            if (!authenticateResult.Succeeded)
            {
                throw new InvalidOperationException(ErrorMessages.ExternalAuthenticationError(authenticateResult));
            }

            ClaimsPrincipal externalUser = authenticateResult.GetPrincipal();
            string externalUserId = externalUser.GetExternalUserId();

            LogExternalClaims(externalUser);

            string externalProvider = authenticateResult.GetExternalProvider();
            Uri returnUrl = authenticateResult.GetReturnUrl();

            var getUserByExternalLoginQuery = new GetUserByExternalLoginQuery(externalProvider, externalUserId);
            Result<User> getUserByExternalLoginResult = await _queryDispatcher.DispatchAsync(getUserByExternalLoginQuery);

            User? user = null;

            if (getUserByExternalLoginResult.Failed)
            {
                if (getUserByExternalLoginResult.Errors.ContainsNotFound())
                {
                    // Check if a local account exists with the same email.
                    string? email = externalUser.GetEmailFromClaims();

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var getUserByEmailQuery = new GetUserByEmailQuery(email);
                        Result<User> getUserByEmailResult = await _queryDispatcher.DispatchAsync(getUserByEmailQuery);

                        if (getUserByEmailResult.Succeeded)
                        {
                            user = getUserByEmailResult.Value;
                        }
                    }

                    // If no user exists with this email, we forward over to registration.
                    // If the user exists, we provide account linking instructions.
                    string page = user != null ? PageConstants.ExternalLoginLink : AccountPageConstants.Register;
                    var routeValues = new { returnUrl };
                    return RedirectToPage(page, routeValues);
                }

                foreach (IError error in getUserByExternalLoginResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Message);
                }
            }

            // At this point a user exists, and we can continue the login flow.
            user = getUserByExternalLoginResult.Value;

            var isUserLockedOutQuery = new IsUserLockedOutQuery(user.UserId);
            bool userIsLockedOut = await _queryDispatcher.DispatchAsync(isUserLockedOutQuery);

            if (userIsLockedOut)
            {
                await HttpContext.SignOutExternalAsync();
                return RedirectToPage(AccountPageConstants.Lockout);
            }

            var doesUserEmailRequireConfirmationQuery = new DoesUserEmailRequireConfirmationQuery(user.UserId);
            bool userEmailRequiresConfirmation = await _queryDispatcher.DispatchAsync(doesUserEmailRequireConfirmationQuery);

            // If we require a confirmed account, we have to short circuit the flow,
            // and provide instructions and a way to resend an email confirmation.
            if (userEmailRequiresConfirmation)
            {
                await HttpContext.SignOutExternalAsync();
                return RedirectToPage(AccountPageConstants.ConfirmEmail, new { userId = user.UserId, returnUrl });
            }

            var signInExternalCommand = new SignInExternalCommand(user.UserId, externalProvider, externalUserId, returnUrl.ToString());
            Result<ExternalSignIn> signInExternalResult = await _commandDispatcher.DispatchAsync(signInExternalCommand);

            AuthorizationRequest? authorizationRequest = signInExternalResult.Succeeded ? 
                signInExternalResult.Value.AuthorizationRequest : null;

            return this.Redirect(authorizationRequest, returnUrl.ToString(), Redirect);
        }
        catch (Exception)
        {
            // Ensure the external cookie is cleared if there is an error here.
            await HttpContext.SignOutExternalAsync();
            throw;
        }
    }

    private void LogExternalClaims(ClaimsPrincipal externalUser)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        IEnumerable<string> externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");

        _logger.ExternalClaims(externalClaims);
    }
}
