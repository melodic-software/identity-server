using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Redirect;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public string? RedirectUri { get; set; }

    public IActionResult OnGet(string? redirectUri)
    {
        if (Url.IsLocalUrl(redirectUri))
        {
            RedirectUri = redirectUri;
            return Page();
        }

        return RedirectToPage(PageConstants.Error);
    }
}
