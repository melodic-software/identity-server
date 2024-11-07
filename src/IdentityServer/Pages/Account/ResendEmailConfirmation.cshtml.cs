using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account;

[AllowAnonymous]
public class ResendEmailConfirmationModel : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;

    public ResendEmailConfirmationModel(
        ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string ReturnUrl { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl ?? Url.Content(PathConstants.RootRelativePath)
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var getUserByEmailQuery = new GetUserByEmailQuery(Input.Email);
        Result<User> getUserByEmailResult = await _queryDispatcher.DispatchAsync(getUserByEmailQuery);

        if (getUserByEmailResult.Failed)
        {
            if (getUserByEmailResult.Errors.ContainsNotFound())
            {
                // This is to ensure we don't leak the details that an account exists or not.
                // TODO: Replace this. It isn't the best way to have a success status message.
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            foreach (IError error in getUserByEmailResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = getUserByEmailResult.Value;

        if (user.EmailConfirmed)
        {
            // TODO: Replace this. It isn't the best way to have a success status message.
            ModelState.AddModelError(string.Empty, "This email is already confirmed.");
            return Page();
        }

        if (user.Email != null)
        {
            var sendConfirmationEmailCommand = new SendConfirmationEmailCommand(user.UserId, Input.ReturnUrl);
            Result<string> result = await _commandDispatcher.DispatchAsync(sendConfirmationEmailCommand);

            if (result.Failed)
            {
                foreach (IError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Message);
                }

                return Page();
            }
        }

        // TODO: Replace this. It isn't the best way to have a success status message.
        ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");

        return Page();
    }
}