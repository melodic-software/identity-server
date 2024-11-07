using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityServer.Security.Authentication.Model;
using IdentityServer.Security.Authentication.Schemes.Abstract;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Security.Authentication.Schemes;

public class AuthenticationSchemeService : IAuthenticationSchemeService
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;

    public AuthenticationSchemeService(IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore)
    {
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
    }

    public async Task<List<AuthenticationScheme>> GetAllSchemesAsync()
    {
        return (await _schemeProvider.GetAllSchemesAsync()).ToList();
    }

    /// <summary>
    /// Returns both static and dynamic providers.
    /// Static providers are registered services in the DI container and can be accessed via <see cref="IAuthenticationSchemeProvider"/>.
    /// Dynamic providers are kept in a data store, and can be accessed via <see cref="IIdentityProviderStore"/>.
    /// https://docs.duendesoftware.com/identityserver/v7/ui/login/dynamicproviders/.
    /// </summary>
    /// <returns>A merged and unified list of external providers.</returns>
    public async Task<List<ExternalProvider>> GetProvidersAsync()
    {
        List<AuthenticationScheme> schemes = await GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ExternalProvider
            (
                authenticationScheme: x.Name,
                displayName: x.DisplayName ?? x.Name
            )).ToList();

        IEnumerable<ExternalProvider> dynamicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ExternalProvider
            (
                authenticationScheme: x.Scheme,
                displayName: x.DisplayName ?? x.Scheme
            ));

        providers.AddRange(dynamicSchemes);

        return providers;
    }

    public List<ExternalProvider> FilterByClient(Client? client, List<ExternalProvider> providers)
    {
        if (client != null && client.IdentityProviderRestrictions.Count != 0)
        {
            providers = providers
                .Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme))
                .ToList();
        }

        return providers;
    }
}
