using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ChangePassword;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserHavePassword;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetLoggedInUser;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.RefreshSignIn;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Manage;

public class ChangePasswordModel : PageModel
{
    private readonly IDispatchCommands _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;

    public ChangePasswordModel(
        IDispatchCommands commandDispatcher,
        IQueryDispatchFacade queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
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

        var doesUserHavePasswordQuery = new DoesUserHavePasswordQuery(user.UserId);
        bool hasPassword = await _queryDispatcher.DispatchAsync(doesUserHavePasswordQuery);

        if (!hasPassword)
        {
            return RedirectToPage(AccountManagementPageConstants.SetPassword);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? userId = User.GetUserIdFromClaims();

        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, "Could not obtain user ID from claims.");
            return Page();
        }

        var changePasswordCommand = new ChangePasswordCommand(userId, Input.OldPassword, Input.NewPassword);
        Result changePasswordResult = await _commandDispatcher.DispatchAsync(changePasswordCommand);

        if (changePasswordResult.Failed)
        {
            if (changePasswordResult.Errors.ContainsNotFound())
            {
                return NotFound($"Unable to load user with ID: '{userId}'.");
            }

            foreach (IError error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        StatusMessage = "Your password has been changed.";

        var refreshSignInCommand = new RefreshSignInCommand();
        Result refreshSignInResult = await _commandDispatcher.DispatchAsync(refreshSignInCommand);

        return RedirectToPage();
    }
}