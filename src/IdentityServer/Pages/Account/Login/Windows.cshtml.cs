using IdentityModel;
using IdentityServer.Constants;
using IdentityServer.Security.Authentication.Schemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Principal;

namespace IdentityServer.Pages.Account.Login;

// https://docs.duendesoftware.com/identityserver/v7/ui/login/windows/
// https://github.com/DuendeSoftware/Samples/blob/main/IdentityServer/v7/UserInteraction/WindowsAuthentication/IdentityServerHost/Pages/Account/Login/Windows.cshtml.cs

[AllowAnonymous]
public class WindowsModel : PageModel
{
#pragma warning disable CA1416
    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        returnUrl ??= PathConstants.RootRelativePath;

        // See if windows auth has already been requested and succeeded.
        AuthenticateResult result = await HttpContext.AuthenticateAsync(AuthenticationSchemeConstants.Windows);

        if (result.Principal is not WindowsPrincipal wp)
        {
            return Challenge(AuthenticationSchemeConstants.Windows);
        }

        var id = new ClaimsIdentity(AuthenticationSchemeConstants.Windows);

        // The sid is a good sub value.
        Claim? sidClaim = wp.FindFirst(ClaimTypes.PrimarySid);

        if (sidClaim?.Value == null)
        {
            throw new Exception("SID claim must be provided.");
        }

        // The sid is a good sub value.
        id.AddClaim(new Claim(JwtClaimTypes.Subject, sidClaim.Value));

        // The account name is the closest we have to a display name.
        if (!string.IsNullOrWhiteSpace(wp.Identity.Name))
        {
            id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));
        }

        if (wp.Identity is not WindowsIdentity wi)
        {
            throw new Exception("Windows identity cannot be null.");
        }

        // Add the groups as claims.
        // Be careful if the number of groups is too large.
        // There is a performance penalty for loading these group claims.

        // Translate group SIDs to display names.
        IdentityReferenceCollection groups = wi.Groups?.Translate(typeof(NTAccount)) ?? new IdentityReferenceCollection();
        IEnumerable<Claim> roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value)).ToList();
        id.AddClaims(roles);

        // We will issue the external cookie and then redirect the user back to the external callback.
        // In essence, we're treating Windows auth the same as any other external authentication mechanism.
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Page(PageConstants.ExternalLoginCallback),
            Items =
            {
                { ParameterNames.Scheme, AuthenticationSchemeConstants.Windows },
                { ParameterNames.ReturnUrl, returnUrl }
            }
        };

        await HttpContext.SignInAsync(
            CookieSchemes.ExternalCookieAuthenticationScheme,
            new ClaimsPrincipal(id),
            props
        );

        if (string.IsNullOrWhiteSpace(props.RedirectUri))
        {
            throw new Exception("Redirect URI is invalid.");
        }

        return Redirect(props.RedirectUri);

        // trigger windows auth
        // since windows auth don't support the redirect uri,
        // this URL is re-triggered when we call challenge
    }
#pragma warning restore CA1416s
}
