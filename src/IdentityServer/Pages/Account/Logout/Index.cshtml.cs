using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using IdentityModel;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.SignOut;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Logout;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly ICommandDispatchFacade _commandDispatcher;
    private readonly IIdentityServerInteractionService _interaction;

    [BindProperty] 
    public string? LogoutId { get; set; }

    public Index(
        ICommandDispatchFacade commandDispatcher,
        IIdentityServerInteractionService interaction)
    {
        _commandDispatcher = commandDispatcher;
        _interaction = interaction;
    }

    public async Task<IActionResult> OnGet(string? logoutId)
    {
        LogoutId = logoutId;

        // Typically the user should be prompted to log out, which requires a POST to remove the cookie.
        // Otherwise, an attacker could hotlink to the logout page causing the user to be automatically logged out.
        // This is why we have a page to prompt the user to logout.
        bool showLogoutPrompt = LogoutOptions.ShowLogoutPrompt;

        if (User.Identity?.IsAuthenticated != true)
        {
            // If the user is not authenticated, then just show logged out page.
            showLogoutPrompt = false;
        }
        else
        {
            LogoutRequest context = await _interaction.GetLogoutContextAsync(LogoutId);

            if (!context.ShowSignoutPrompt)
            {
                // It's safe to automatically sign-out.
                showLogoutPrompt = false;
            }
        }
            
        if (!showLogoutPrompt)
        {
            // If the request for logout was properly authenticated from IdentityServer,
            // then we don't need to show the prompt and can just log the user out directly.
            return await OnPost();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // Clear the existing external cookie.
        // This should already be cleared, but this will ensure it has been cleared.
        await HttpContext.SignOutExternalAsync();

        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage(AccountPageConstants.LoggedOut, new { logoutId = LogoutId });
        }

        // If there's no current logout context, we need to create one.
        // This captures necessary info from the current logged-in user.
        // This can still return null if there is no context needed.
        LogoutId ??= await _interaction.CreateLogoutContextAsync();

        var signOutCommand = new SignOutCommand();
        await _commandDispatcher.DispatchAsync(signOutCommand);

        // See if we need to trigger federated logout.
        string? idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

        // If it was a local login, we can ignore this workflow.

        // Federated logout will NOT be attempted if it was a local login or the scheme doesn't support signout.
        if (idp == null || idp == IdentityServerConstants.LocalIdentityProvider ||
            !await HttpContext.SchemeSupportsSignOutAsync(idp))
        {
            return RedirectToPage(AccountPageConstants.LoggedOut, new { logoutId = LogoutId });
        }

        // https://docs.duendesoftware.com/identityserver/v7/ui/logout/external/

        // We need to see if the provider supports external logout.
        // Build a return URL so the upstream provider will redirect back to us after the user has logged out.
        // This allows us to then complete our single sign-out processing.
        string? url = Url.Page(AccountPageConstants.LoggedOut, new { logoutId = LogoutId });
            
        var authenticationProperties = new AuthenticationProperties { RedirectUri = url };
            
        // This triggers a redirect to the external provider for sign-out.
        return SignOut(authenticationProperties, idp);
    }
}
