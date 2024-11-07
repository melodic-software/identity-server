using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account;

[AllowAnonymous]
public class EmailConfirmationSentModel : PageModel
{
    public string? UserId { get; set; }
    public string? ReturnUrl { get; set; }

    public IActionResult OnGetAsync(string? userId = null, string? returnUrl = null)
    {
        UserId = userId;
        ReturnUrl = returnUrl;
        return Page();
    }
}
