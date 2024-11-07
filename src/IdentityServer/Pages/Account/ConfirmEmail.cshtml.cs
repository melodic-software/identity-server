using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.ReturnUrls.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Constants;
using IdentityServer.Logging;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserSignedIn;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly IIdentityServerInteractionService _interactionService;
    private readonly ILogger<ConfirmEmailModel> _logger;

    [BindProperty]
    public ViewModel View { get; set; }

    public class ViewModel
    {
        public string? UserId { get; set; }
        public string? ReturnUrl { get; set; }
        public bool ShowSendConfirmationEmailLink { get; set; }
        public string StatusMessage { get; set; }
        public bool AutoRedirectEnabled { get; set; } = true;
        public bool CanRedirect => !string.IsNullOrWhiteSpace(ReturnUrl) && !ShowSendConfirmationEmailLink;
        public int RedirectDelayInSeconds { get; set; } = 5;
    }

    public ConfirmEmailModel(
        ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher,
        IIdentityServerInteractionService interactionService,
        ILogger<ConfirmEmailModel> logger)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _interactionService = interactionService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? userId = null, string? token = null, string? returnUrl = null)
    {
        View = new ViewModel
        {
            UserId = userId,
            ReturnUrl = returnUrl,
            ShowSendConfirmationEmailLink = string.IsNullOrWhiteSpace(token)
        };

        if (string.IsNullOrWhiteSpace(View.UserId))
        {
            return RedirectToPage(PageConstants.Home);
        }

        var getUserByIdQuery = new GetUserByIdQuery(View.UserId);
        Result<User> userQueryResult = await _queryDispatcher.DispatchAsync(getUserByIdQuery);

        if (userQueryResult.HasErrors)
        {
            if (userQueryResult.Errors.ContainsNotFound())
            {
                return NotFound($"Unable to load user with ID '{View.UserId}'.");
            }

            foreach (IError error in userQueryResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = userQueryResult.Value;

        if (user.EmailConfirmed)
        {
            View.StatusMessage = "Email has already been confirmed.";
            View.ReturnUrl = View.ReturnUrl ??= PageConstants.Home;
            View.ShowSendConfirmationEmailLink = false;
            return Page();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            // We just want to show a link that users can click and send a new confirmation.
            // See page handler below.
            return Page();
        }

        var confirmEmailCommand = new ConfirmEmailCommand(user.UserId, token);
        Result confirmEmailResult = await _commandDispatcher.DispatchAsync(confirmEmailCommand);

        if (confirmEmailResult.HasErrors)
        {
            ModelState.AddModelError(string.Empty, "Error confirming your email.");
            return Page();
        }

        View.StatusMessage = "Thank you for confirming your email.";

        var isUserSignedInQuery = new IsUserSignedInQuery();
        bool userIsSignedIn = await _queryDispatcher.DispatchAsync(isUserSignedInQuery);

        if (userIsSignedIn)
        {
            return Page();
        }

        var signInCommand = new SignInCommand(user.UserId, externalProvider: null, externalUserId: null, rememberLogin: false, returnUrl);
        Result<bool> signInResult = await _commandDispatcher.DispatchAsync(signInCommand);

        if (signInResult.HasErrors)
        {
            View.StatusMessage += " We tried to sign you in but there was an error.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmEmailAsync(string userId, string returnUrl)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        Result<User> userQueryResult = await _queryDispatcher.DispatchAsync(getUserByIdQuery);

        if (userQueryResult.HasErrors)
        {
            if (userQueryResult.Errors.ContainsNotFound())
            {
                return NotFound($"Unable to load user with ID '{View.UserId}'.");
            }

            foreach (IError error in userQueryResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = userQueryResult.Value;

        if (user == null)
        {
            throw new InvalidOperationException(ErrorMessages.UnknownUserId);
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException(ErrorMessages.UserMissingEmailAddress);
        }

        if (user.EmailConfirmed)
        {
            return RedirectToPage(AccountPageConstants.ConfirmEmail, new { userId, returnUrl });
        }

        var sendEmailConfirmationCommand = new SendConfirmationEmailCommand(userId, returnUrl);
        Result<string> sendEmailConfirmationResult = await _commandDispatcher.DispatchAsync(sendEmailConfirmationCommand);

        if (sendEmailConfirmationResult.Succeeded)
        {
            return RedirectToPage(AccountPageConstants.EmailConfirmationSent, new { userId, returnUrl });
        }

        ModelState.AddModelError(string.Empty, "There was an error sending confirmation email.");
        return Page();

    }

    public IActionResult OnPostRedirect(string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return Page();
        }

        if (Url.IsLocalUrl(returnUrl) || _interactionService.IsValidReturnUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        _logger.LogInvalidReturnUrl(returnUrl);

        throw new ArgumentException(ErrorMessages.InvalidReturnUrl);
    }
}
