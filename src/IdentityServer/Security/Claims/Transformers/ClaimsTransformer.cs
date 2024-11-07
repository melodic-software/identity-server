using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Security.Claims.Transformers;

[Obsolete("This is something that is typically done at the client. IdentityServer has its own claim transformation process.")]
public class ClaimsTransformer : IClaimsTransformation
{
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _userEmailStore;

    public ClaimsTransformer(
        IUserStore<ApplicationUser> userStore,
        IUserEmailStore<ApplicationUser> userEmailStore)
    {
        _userStore = userStore;
        _userEmailStore = userEmailStore;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ClaimsPrincipal clonedPrincipal = principal.Clone();

        // This is the primary identity.
        if (clonedPrincipal.Identity == null)
        {
            return clonedPrincipal;
        }

        var claimsIdentity = (ClaimsIdentity)clonedPrincipal.Identity;

        Claim? subjectClaim = claimsIdentity.FindFirst(JwtClaimTypes.Subject);
        Claim? nameIdentifierClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        Claim? emailClaim = claimsIdentity.FindFirst(JwtClaimTypes.Email) ?? 
                            claimsIdentity.FindFirst(ClaimTypes.Email);

        // Claims can be added here.
        // Always check if they exist first.
        string? userId = subjectClaim?.Value ?? nameIdentifierClaim?.Value;
        string? email = emailClaim?.Value;

        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            user = await _userStore.FindByIdAsync(userId, CancellationToken.None);
        }

        if (user == null && !string.IsNullOrWhiteSpace(email))
        {
            user = await _userEmailStore.FindByEmailAsync(email, CancellationToken.None);
        }

        if (user != null)
        {
            // Claims can be pulled from the user record itself if it already doesn't exist.
            // The claims in the UserClaims table will likely already be populated in the identity claims.
        }

        Claim? givenNameClaim = claimsIdentity.FindFirst(ClaimTypes.GivenName);
        Claim? surnameClaim = claimsIdentity.FindFirst(ClaimTypes.Surname);
        Claim? nameClaim = claimsIdentity.FindFirst(ClaimTypes.Name);

        return clonedPrincipal;
    }
}
