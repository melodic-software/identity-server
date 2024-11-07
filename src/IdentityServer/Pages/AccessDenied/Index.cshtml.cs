using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.AccessDenied;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public string ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content(PathConstants.RootRelativePath);
    }
}
