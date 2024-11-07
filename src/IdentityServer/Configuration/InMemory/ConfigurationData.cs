using Duende.IdentityServer.Models;
using IdentityModel;

namespace IdentityServer.Configuration.InMemory;

public static class ConfigurationData
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResources.Address(),
        new IdentityResources.Phone(),
        new(
            "roles",
            "Your role(s)",
            [JwtClaimTypes.Role]
        ),
        new(
            "permissions",
            "Your permission(s)",
            ["permission"]
        ),
        new(
            "country", 
            "The country you're living in",
            ["country"]
        ),
        //new("favorite_color", ["Your favorite color."])
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new()
        {
            Name = ApiScopeConstants.IdentityServer,
            DisplayName = "Identity Server API",
            ShowInDiscoveryDocument = false
        },
        new()
        {
            Name = ApiScopeConstants.Medley,
            DisplayName = "Medley API",
            ShowInDiscoveryDocument = true
        }
    ];
}
