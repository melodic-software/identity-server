using IdentityModel;
using IdentityServer.AspNetIdentity.Email;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace IdentityServer.AspNetIdentity.Users.Provisioning;

public static class UserProvisioningService
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection", Justification = "<Pending>")]
    public static async Task<ApplicationUser> AutoProvisionUserAsync(IUrlHelper urlHelper, HttpContext httpContext, string provider, string providerUserId, IEnumerable<Claim> claims, UserManager<ApplicationUser> userManager, EmailService emailService, string returnUrl)
    {
        string sub = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = sub,
            // Don't need a username, since the user will be using an external provider to login.
            UserName = null
        };

        // Email
        string? email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
                        claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (email != null)
        {
            user.Email = email;
            user.UserName = email;
        }

        // Create a list of claims that we want to transfer into our store.
        var filtered = new List<Claim>();

        // The user's display name.
        string? name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                       claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        if (name != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Name, name));
        }
        else
        {
            string? first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                            claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;

            string? last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                           claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

            if (first != null && last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
            }
            else if (first != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first));
            }
            else if (last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, last));
            }
        }

        // Set this to false by default.
        // We do not allow auto confirmation when automatically provisioning users.
        user.EmailConfirmed = false;

        IdentityResult identityResult = await userManager.CreateAsync(user);

        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(identityResult.Errors.First().Description);
        }
        
        if (email != null)
        {
            await emailService.SendConfirmationEmailAsync(urlHelper, httpContext, user, email, returnUrl);
        }

        if (filtered.Count != 0)
        {
            identityResult = await userManager.AddClaimsAsync(user, filtered);

            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(identityResult.Errors.First().Description);
            }
        }

        identityResult = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));

        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(identityResult.Errors.First().Description);
        }

        return user;
    }
}
