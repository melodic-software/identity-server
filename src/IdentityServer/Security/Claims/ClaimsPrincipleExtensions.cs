using IdentityModel;
using IdentityServer.Constants;
using System.Security.Claims;

namespace IdentityServer.Security.Claims;

public static class ClaimsPrincipleExtensions
{
    /// <summary>
    /// Try to determine the unique id of the external user (issued by the provider).
    /// The most common claim type for that are the sub claim and the NameIdentifier.
    /// Depending on the external provider, some other claim type might be used.
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public static string GetExternalUserId(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal.FindFirstValue(JwtClaimTypes.Subject) ??
        claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ??
        // If the external provider uses another claim, this code must be updated.
        throw new InvalidOperationException(ErrorMessages.UnknownUserId);
}
