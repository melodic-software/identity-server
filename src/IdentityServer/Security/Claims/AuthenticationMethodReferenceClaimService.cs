using Authsignal;
using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using System.Security.Claims;

namespace IdentityServer.Security.Claims;

public static class AuthenticationMethodReferenceClaimService
{
    public static async Task AddAmrClaimAsync(ClaimsIdentity? claimsIdentity, ApplicationUser user, 
        IConfiguration configuration, IAuthsignalClient authsignalClient)
    {
        if (claimsIdentity == null)
        {
            return;
        }

        bool hasAmrClaim = claimsIdentity.FindFirst(JwtClaimTypes.AuthenticationMethod) != null ||
                           claimsIdentity.FindFirst(ClaimTypes.AuthenticationMethod) != null;

        if (hasAmrClaim)
        {
            return;
        }

        bool enrolledInExternalMfa = false;

        if (configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false))
        {
            UserResponse userResponse = await authsignalClient.GetUser(new UserRequest(user.Id));

            enrolledInExternalMfa = userResponse.IsEnrolled;
        }

        if (user.TwoFactorEnabled || enrolledInExternalMfa)
        {
            claimsIdentity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, "mfa"));
        }
        else
        {
            claimsIdentity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, "pwd"));
        }
    }
}
