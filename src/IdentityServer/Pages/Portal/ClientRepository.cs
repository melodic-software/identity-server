using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Pages.Portal;

public class ThirdPartyInitiatedLoginLink
{
    public string? LinkText { get; set; }
    public string? InitiateLoginUri { get; set; }
}


public class ClientRepository
{
    private readonly ConfigurationDbContext _context;

    public ClientRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ThirdPartyInitiatedLoginLink>> GetClientsWithLoginUris(string? filter = null)
    {
        IQueryable<Client> query = _context.Clients
            .Where(c => c.InitiateLoginUri != null);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x => x.ClientId.Contains(filter) || x.ClientName.Contains(filter));
        }

        IQueryable<ThirdPartyInitiatedLoginLink> result = query.Select(c => new ThirdPartyInitiatedLoginLink
        {
            LinkText = string.IsNullOrWhiteSpace(c.ClientName) ? c.ClientId : c.ClientName,
            InitiateLoginUri = c.InitiateLoginUri
        });

        return await result.ToArrayAsync();
    }
}
