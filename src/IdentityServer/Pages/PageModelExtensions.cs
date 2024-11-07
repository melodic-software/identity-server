using Enterprise.Http.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages;

public static class PageModelExtensions
{
    /// <summary>
    /// Renders a loading page that is used to redirect back to the redirectUri.
    /// </summary>
    public static IActionResult LoadingPage(this PageModel page, string? redirectUri)
    {
        page.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        page.HttpContext.Response.Headers[HttpHeaderNames.Location] = string.Empty;

        return page.RedirectToPage(PageConstants.Redirect, new { RedirectUri = redirectUri });
    }
}
