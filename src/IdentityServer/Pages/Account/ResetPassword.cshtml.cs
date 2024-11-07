using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Pages.Account.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IdentityServer.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;

    public ResetPasswordModel(ICommandDispatchFacade commandDispatcher,
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

        [Required]
        // TODO: These should come from the dynamic ASP.NET Identity configuration.
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }

    public IActionResult OnGet(string? code = null)
    {
        if (code == null)
        {
            throw new Exception("A code must be supplied for password reset.");
        }

        byte[] codeBytes = WebEncoders.Base64UrlDecode(code);

        Input = new InputModel
        {
            Code = Encoding.UTF8.GetString(codeBytes)
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var query = new GetUserByEmailQuery(Input.Email);
        Result<User> queryResult = await _queryDispatcher.DispatchAsync(query);

        if (queryResult.Failed)
        {
            if (queryResult.Errors.ContainsNotFound())
            {
                // Don't reveal that the user does not exist.
                return RedirectToPage(AccountManagementPageConstants.ResetPasswordConfirmation);
            }

            foreach (IError error in queryResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = queryResult.Value;

        var command = new ResetPasswordCommand(user.UserId, Input.Code, Input.ConfirmPassword);
        Result commandResult = await _commandDispatcher.DispatchAsync(command);

        if (commandResult.Succeeded)
        {
            return RedirectToPage(AccountManagementPageConstants.ResetPasswordConfirmation);
        }

        foreach (IError error in commandResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Message);
        }

        return Page();
    }
}