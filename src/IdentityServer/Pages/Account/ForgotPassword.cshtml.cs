using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Passwords.SendPasswordResetEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly ICommandDispatchFacade _commandDispatcher;

    public ForgotPasswordModel(
        IQueryDispatchFacade queryDispatcher,
        ICommandDispatchFacade commandDispatcher)
    {
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Review and implement best practices.
        // https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html

        var query = new GetUserByEmailQuery(Input.Email);
        Result<User> queryResult = await _queryDispatcher.DispatchAsync(query);

        if (queryResult.Failed)
        {
            if (queryResult.Errors.ContainsNotFound())
            {
                // Don't reveal that the user does not exist.
                return RedirectToPage(AccountPageConstants.ForgotPasswordConfirmation);
            }

            foreach (IError error in queryResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = queryResult.Value;

        if (!user.EmailConfirmed)
        {
            // Don't reveal that the user's email is not confirmed.
            return RedirectToPage(AccountPageConstants.ForgotPasswordConfirmation);
        }

        var command = new SendPasswordResetEmailCommand(user.UserId);
        Result commandResult = await _commandDispatcher.DispatchAsync(command);

        if (commandResult.Succeeded)
        {
            return RedirectToPage(AccountPageConstants.ForgotPasswordConfirmation);
        }

        foreach (IError error in commandResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Message);
        }

        return Page();
    }
}