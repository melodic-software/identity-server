using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendEmailChangeConfirmationEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetLoggedInUser;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Security.Mfa.AuthSignal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly AuthsignalActionResultService _authsignalActionResultService;

    public EmailModel(
        ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher,
        AuthsignalActionResultService authsignalActionResultService)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _authsignalActionResultService = authsignalActionResultService;
    }

    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string? NewEmail { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? token = null)
    {
        var getLoggedInUserQuery = new GetLoggedInUserQuery();
        Result<User> getLoggedInUserResult = await _queryDispatcher.DispatchAsync(getLoggedInUserQuery);

        if (getLoggedInUserResult.Failed)
        {
            if (getLoggedInUserResult.Errors.ContainsNotFound())
            {
                return NotFound("The logged in user could not be obtained.");
            }

            foreach (IError error in getLoggedInUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = getLoggedInUserResult.Value;

        string? redirectUrl = Url.PageLink(AccountManagementPageConstants.ManageEmail);

        IActionResult? mfaActionResult = await _authsignalActionResultService
            .HandlePageMfa(this, user, "manage-email", redirectUrl, token, Redirect);

        if (mfaActionResult != null)
        {
            return mfaActionResult;
        }

        PopulateModel(user);

        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var getLoggedInUserQuery = new GetLoggedInUserQuery();
        Result<User> getLoggedInUserResult = await _queryDispatcher.DispatchAsync(getLoggedInUserQuery);

        if (getLoggedInUserResult.Failed)
        {
            if (getLoggedInUserResult.Errors.ContainsNotFound())
            {
                return NotFound("The logged in user could not be obtained.");
            }

            foreach (IError error in getLoggedInUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = getLoggedInUserResult.Value;

        if (!ModelState.IsValid)
        {
            PopulateModel(user);
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.NewEmail))
        {
            ModelState.AddModelError(nameof(Input.NewEmail), "Please provide a new email.");
            PopulateModel(user);
            return Page();
        }

        var sendEmailChangeConfirmationEmailCommand = new SendEmailChangeConfirmationEmailCommand(user.UserId, Input.NewEmail);
        Result<string> sendEmailChangeConfirmationEmailResult = await _commandDispatcher.DispatchAsync(sendEmailChangeConfirmationEmailCommand);

        if (sendEmailChangeConfirmationEmailResult.Failed)
        {
            IError? unchangedEmailError = sendEmailChangeConfirmationEmailResult.Errors.Find(x => x.Code == "NewEmail.MustBeDifferent");
            StatusMessage = unchangedEmailError != null ? "Your email is unchanged." : "An error occured.";
            return RedirectToPage();
        }
        
        if (sendEmailChangeConfirmationEmailResult.Succeeded)
        {
            StatusMessage = "Confirmation link to change email sent. Please check your email.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var getLoggedInUserQuery = new GetLoggedInUserQuery();
        Result<User> getLoggedInUserResult = await _queryDispatcher.DispatchAsync(getLoggedInUserQuery);

        if (getLoggedInUserResult.Failed)
        {
            if (getLoggedInUserResult.Errors.ContainsNotFound())
            {
                return NotFound("The logged in user could not be obtained.");
            }

            foreach (IError error in getLoggedInUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = getLoggedInUserResult.Value;

        if (!ModelState.IsValid)
        {
            PopulateModel(user);
            return Page();
        }

        var sendConfirmationEmailCommand = new SendConfirmationEmailCommand(user.UserId, returnUrl: null);
        Result<string> sendConfirmationEmailResult = await _commandDispatcher.DispatchAsync(sendConfirmationEmailCommand);

        StatusMessage = "Verification email sent. Please check your email.";

        return RedirectToPage();
    }

    private void PopulateModel(User user)
    {
        Email = user.Email;

        Input = new InputModel
        {
            NewEmail = user.Email,
        };

        IsEmailConfirmed = user.EmailConfirmed;
    }
}