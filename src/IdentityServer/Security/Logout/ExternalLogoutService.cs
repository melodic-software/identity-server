using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Pages.Account;
using IdentityServer.Security.Authentication.Schemes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Security.Logout;

public static class ExternalLogoutService
{
    // IActionResult? result = await ExternalLogoutService.LogoutGoogle(this, idp, LogoutId, _userManager, _logger)

    public static async Task<IActionResult?> LogoutGoogle(PageModel page, string idp, string? logoutId, UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        if (idp != AuthenticationSchemeConstants.Google)
        {
            return null;
        }

        ApplicationUser user = await userManager.GetUserAsync(page.User) ??
                               throw new InvalidOperationException(ErrorMessages.UserNotFound);

        string? accessToken = await userManager.GetAuthenticationTokenAsync(user, idp, "access_token");

        if (string.IsNullOrEmpty(accessToken))
        {
            return page.RedirectToPage(AccountPageConstants.LoggedOut, new { logoutId });
        }

        // Revoke the token to invalidate the session on Google's end
        // TODO: Use HttpClientFactory
        using var client = new HttpClient();
        string requestUri = $"https://accounts.google.com/o/oauth2/revoke?token={accessToken}";
        HttpResponseMessage response = await client.PostAsync(requestUri, null);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to revoke Google token.");
        }

        // Redirect to the logged-out page after revoking the token
        return page.RedirectToPage(AccountPageConstants.LoggedOut, new { logoutId });
    }

    // IActionResult? result = ExternalLogoutService.LogoutMicrosoft(this, idp, LogoutId, _userManager, _logger, Redirect);

    public static IActionResult? LogoutMicrosoft(PageModel page, string idp, string? logoutId, Func<string, IActionResult> redirect)
    {
        if (idp != AuthenticationSchemeConstants.Microsoft)
        {
            return null;
        }

        // Redirect to the Microsoft logout endpoint.
        string microsoftLogoutUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/logout";
        string? postLogoutRedirectUri = page.Url.Page(AccountPageConstants.LoggedOut, new { logoutId });
        string redirectUri = $"{microsoftLogoutUrl}?post_logout_redirect_uri={postLogoutRedirectUri}";

        IActionResult actionResult = redirect(redirectUri);

        return actionResult;
    }
}
