using Authsignal;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.ReturnUrls.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Constants;
using IdentityServer.Logging;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;
using IdentityServer.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace IdentityServer.Pages.Account.Login;

// This was added to accomodate Authsignal MFA.

[AllowAnonymous]
public class Callback : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IAuthsignalClient _authsignalClient;
    private readonly ILogger<Callback> _logger;

    public Callback(ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher,
        IIdentityServerInteractionService interaction,
        IAuthsignalClient authsignalClient,
        ILogger<Callback> logger)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _interaction = interaction;
        _authsignalClient = authsignalClient;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(string? returnUrl, string? token, bool rememberLogin)
    {
        // Decode the return URL.
        string? decodedReturnUrl = !string.IsNullOrWhiteSpace(returnUrl)
            ? Encoding.UTF8.GetString(Convert.FromBase64String(returnUrl))
            : null;

        // Redirect to login page if token is missing.
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage(AccountPageConstants.Login, new { returnUrl = decodedReturnUrl });
        }

        // Get authorization context if available.
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(decodedReturnUrl);

        // Validate the MFA challenge.
        var validateChallengeRequest = new ValidateChallengeRequest(token);
        ValidateChallengeResponse validateChallengeResponse = await _authsignalClient.ValidateChallenge(validateChallengeRequest);

        if (validateChallengeResponse.State != UserActionState.CHALLENGE_SUCCEEDED)
        {
            // TODO: Handle different states better.
            // If MFA is required for user (AspNetIdentity flag), they need to know that they have to enroll and use MFA.
            return RedirectToPage(AccountPageConstants.Login, new { returnUrl = decodedReturnUrl });
        }

        // Get the user ID from the validation response.
        string? userId = validateChallengeResponse.UserId;

        // Check if user ID is valid.
        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidUserIdErrorMessage);
            return Page();
        }

        var getUserByIdQuery = new GetUserByIdQuery(userId);
        Result<User> getUserByIdQueryResult = await _queryDispatcher.DispatchAsync(getUserByIdQuery);

        // It is best practice to not inform the user if an account exists or not.
        // Here we just inform them that the credentials are not valid.
        if (getUserByIdQueryResult.Errors.ContainsNotFound())
        {
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
            return Page();
        }

        var signInCommand = new SignInCommand(userId, null, null, rememberLogin, returnUrl);
        Result<bool> signInCommandResult = await _commandDispatcher.DispatchAsync(signInCommand);

        if (signInCommandResult.Failed)
        {
            foreach (IError error in signInCommandResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        // Handle return URL or redirect to home page.
        if (authorizationRequest != null)
        {
            // This "can't happen", because if the ReturnUrl was null, then the context would be null.
            ArgumentNullException.ThrowIfNull(decodedReturnUrl);

            // We can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null.
            return this.Redirect(authorizationRequest, decodedReturnUrl, Redirect);
        }

        // Request for a local page.
        if (Url.IsLocalUrl(decodedReturnUrl))
        {
            return Redirect(decodedReturnUrl);
        }

        if (string.IsNullOrEmpty(decodedReturnUrl))
        {
            return Redirect(PathConstants.RootRelativePath);
        }

        _logger.LogInvalidReturnUrl(returnUrl);

        throw new ArgumentException(ErrorMessages.InvalidReturnUrl);
    }
}
