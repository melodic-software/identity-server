using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using IdentityServer.Constants;
using IdentityServer.Security.Authentication.Results;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServer.Pages.ExternalLogin;

[AllowAnonymous]
public class LinkModel : PageModel
{
    [TempData]
    public string Email { get; set; }

    [TempData]
    public string? ExternalProvider { get; set; }

    [TempData]
    public string? ExternalUserId { get; set; }

    [TempData]
    public string ReturnUrl { get; set; }

    [TempData]
    public string Message { get; set; }

    public async Task OnGet(string? returnUrl = null)
    {
        // Restore values from TempData if they exist.
        Email = TempData[nameof(Email)] as string ?? Email;
        ExternalProvider = TempData[nameof(ExternalProvider)] as string ?? ExternalProvider;
        ExternalUserId = TempData[nameof(ExternalUserId)] as string ?? ExternalUserId;
        ReturnUrl = TempData[nameof(ReturnUrl)] as string ?? returnUrl ?? Url.Content(PathConstants.RootRelativePath);
        Message = TempData[nameof(Message)] as string ?? Message;

        AuthenticateResult result = await HttpContext.AuthenticateExternalAsync();

        if (result.Succeeded)
        {
            ClaimsPrincipal externalUser = result.GetPrincipal();

            Email = externalUser.GetEmailFromClaims() ??
                    throw new InvalidOperationException(ErrorMessages.ExternalIdentityInvalidEmailAddress);
            
            ExternalProvider = result.Properties.Items[ParameterNames.Scheme] ??
                               throw new InvalidOperationException(ErrorMessages.AuthPropertiesNullScheme);

            ExternalUserId = externalUser.GetExternalUserId();

            ReturnUrl = result.Properties.Items[ParameterNames.ReturnUrl] ?? ReturnUrl;

            Message = $"An account with the email <strong>{Email}</strong> already exists and has not yet been linked to a {ExternalProvider} account.";

            await HttpContext.SignOutExternalAsync();
        }
        else if (string.IsNullOrWhiteSpace(Message))
        {
            Message = "An account already exists with the email address associated with this login provider and has not yet been linked.";
        }
        
        // Retain TempData values so they persist across refreshes.
        TempData.Keep(nameof(Email));
        TempData.Keep(nameof(ExternalProvider));
        TempData.Keep(nameof(ExternalUserId));
        TempData.Keep(nameof(ReturnUrl));
        TempData.Keep(nameof(Message));
    }
}
