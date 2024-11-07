using IdentityModel;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

public static class UserClaimsExtensions
{
    public static string GetFirstName(this IEnumerable<Claim> claims) =>
        claims.First(x => x.Type == JwtClaimTypes.GivenName).Value;

    public static string GetLastName(this IEnumerable<Claim> claims) =>
        claims.First(x => x.Type == JwtClaimTypes.FamilyName).Value;

    public static void SetUserRegistrationClaims(this IList<Claim> claims, string? firstName, string? lastName)
    {
        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
        {
            claims.Add(new Claim(JwtClaimTypes.Name, $"{firstName} {lastName}"));
        }
        else if (!string.IsNullOrWhiteSpace(firstName))
        {
            claims.Add(new Claim(JwtClaimTypes.Name, firstName));
        }
        else if (!string.IsNullOrWhiteSpace(lastName))
        {
            claims.Add(new Claim(JwtClaimTypes.Name, lastName));
        }

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            claims.Add(new Claim(JwtClaimTypes.GivenName, firstName));
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            claims.Add(new Claim(JwtClaimTypes.FamilyName, lastName));
        }
    }
}
