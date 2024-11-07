using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account;

public class ConfirmEmailChangeModel : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;

    public ConfirmEmailChangeModel(
        ICommandDispatchFacade commandDispatcher,
        IQueryDispatchFacade queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? email, string? code)
    {
        if (userId == null || email == null || code == null)
        {
            return RedirectToPage(PageConstants.Home);
        }

        var getUserByIdQuery = new GetUserByIdQuery(userId);
        Result<User> userQueryResult = await _queryDispatcher.DispatchAsync(getUserByIdQuery);

        if (userQueryResult.HasErrors)
        {
            if (userQueryResult.Errors.ContainsNotFound())
            {
                return NotFound($"Unable to load user with ID \"{userId}\".");
            }

            foreach (IError error in userQueryResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }
        
        var confirmEmailChangeCommand = new ConfirmEmailChangeCommand(userId, email, code);
        Result confirmEmailChangeResult = await _commandDispatcher.DispatchAsync(confirmEmailChangeCommand);

        if (confirmEmailChangeResult.HasErrors)
        {
            IError firstError = confirmEmailChangeResult.FirstError;
            StatusMessage = firstError.Message;
            return Page();
        }
        
        StatusMessage = "Thank you for confirming your email change.";
        return Page();
    }
}