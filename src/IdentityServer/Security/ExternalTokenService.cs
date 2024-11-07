using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Security;

public static class ExternalTokenService
{
    public static async Task StoreExternalTokensAsync(ApplicationUser user, AuthenticateResult result, UserManager<ApplicationUser> userManager)
    {
        IEnumerable<AuthenticationToken> tokens = result.Properties?.GetTokens().ToList() ?? [];

        if (!tokens.Any())
        {
            return;
        }

        string scheme = result.Properties?.Items["scheme"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(scheme))
        {
            return;
        }

        foreach (AuthenticationToken token in tokens)
        {
            await userManager.SetAuthenticationTokenAsync(user, scheme, token.Name, token.Value);
        }
    }
}
