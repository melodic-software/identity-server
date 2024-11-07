using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Security.Claims;

public static class ExternalClaimsSyncService
{
    private static readonly Dictionary<string, string?> ClaimsToSync = new()
    {
        { "urn:google:sub", null },
        { "urn:google:name", null },
        { "urn:google:given_name", null },
        { "urn:google:family_name", null },
        { "urn:google:profile", null },
        { "urn:google:picture", null },
        { "urn:google:email", null },
        { "urn:google:email_verified", null },
        { "urn:google:locale", null },
        { "urn:google:hd", null },
        { "urn:microsoft:sub", null },
        { "urn:microsoft:name", null },
        { "urn:microsoft:given_name", null },
        { "urn:microsoft:family_name", null },
        { "urn:microsoft:profile", null },
        { "urn:microsoft:email", null },
        { "urn:microsoft:email_verified", null },
        { "urn:microsoft:locale", null }
    };

    public static async Task SyncExternalClaimsAsync(ApplicationUser user, AuthenticateResult result, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (ClaimsToSync.Count <= 0 || !result.Succeeded)
        {
            return;
        }

        IList<Claim> userClaims = await userManager.GetClaimsAsync(user);

        bool refreshSignIn = false;

        foreach (KeyValuePair<string, string?> addedClaim in ClaimsToSync)
        {
            Claim? userClaim = userClaims.FirstOrDefault(c => c.Type == addedClaim.Key);

            if (result.Principal.HasClaim(c => c.Type == addedClaim.Key))
            {
                Claim? externalClaim = result.Principal.FindFirst(addedClaim.Key);

                if (externalClaim == null)
                {
                    continue;
                }

                if (userClaim == null)
                {
                    await userManager.AddClaimAsync(user, new Claim(addedClaim.Key, externalClaim.Value));
                    refreshSignIn = true;
                }
                else if (userClaim.Value != externalClaim.Value)
                {
                    await userManager.ReplaceClaimAsync(user, userClaim, externalClaim);
                    refreshSignIn = true;
                }
            }
            else if (userClaim == null && !string.IsNullOrWhiteSpace(addedClaim.Value))
            {
                // Fill with a default value.
                await userManager.AddClaimAsync(user, new Claim(addedClaim.Key, addedClaim.Value));
                refreshSignIn = true;
            }
        }

        if (refreshSignIn)
        {
            await signInManager.RefreshSignInAsync(user);
        }
    }
}
