using Duende.IdentityServer.Models;
using IdentityServer.Pages;
using IdentityServer.Security.Authorization.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Security.Authorization;

public static class PageRedirectionExtensions
{
    public static IActionResult Redirect(this PageModel page,
        AuthorizationRequest? authRequest,
        string returnUrl,
        Func<string, IActionResult> redirect)
    {
        if (authRequest != null && authRequest.IsNativeClient())
        {
            // The client is native, so this change in how to return the response is for better UX for the end user.
            return page.LoadingPage(returnUrl);
        }

        return redirect(returnUrl);
    }
}
