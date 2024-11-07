using Duende.IdentityServer.Models;
using IdentityServer.Security.Authentication.Model;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Security.Authentication.Schemes.Abstract;

public interface IAuthenticationSchemeService
{
    public Task<List<AuthenticationScheme>> GetAllSchemesAsync();
    public Task<List<ExternalProvider>> GetProvidersAsync();
    List<ExternalProvider> FilterByClient(Client? client, List<ExternalProvider> providers);
}
