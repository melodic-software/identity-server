using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Client = Duende.IdentityServer.EntityFramework.Entities.Client;

namespace IdentityServer.Pages.Admin.Clients;

public class ClientSummaryModel
{
    [Required]
    public string ClientId { get; set; } = default!;
    public string? Name { get; set; }
    [Required]
    public Flow Flow { get; set; }
}

public class CreateClientModel : ClientSummaryModel
{
    public string Secret { get; set; } = default!;
}

public class ClientModel : ClientSummaryModel, IValidatableObject
{
    [Required]
    public string AllowedScopes { get; set; } = default!;

    public string? RedirectUri { get; set; }
    public string? InitiateLoginUri { get; set; }
    public string? PostLogoutRedirectUri { get; set; }
    public string? FrontChannelLogoutUri { get; set; }
    public string? BackChannelLogoutUri { get; set; }

    private static readonly string[] MemberNames = ["RedirectUri"];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();

        if (Flow == Flow.CodeFlowWithPkce && RedirectUri == null)
        {
            errors.Add(new ValidationResult("Redirect URI is required.", MemberNames));
        }

        return errors;
    }
}

public enum Flow
{
    ClientCredentials,
    CodeFlowWithPkce
}

public class ClientRepository
{
    private readonly ConfigurationDbContext _context;

    public ClientRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClientSummaryModel>> GetAllAsync(string? filter = null)
    {
        string[] grants = [GrantType.AuthorizationCode, GrantType.ClientCredentials];

        IQueryable<Client> query = _context.Clients
            .Include(x => x.AllowedGrantTypes)
            .Where(x => x.AllowedGrantTypes.Count == 1 && x.AllowedGrantTypes.Any(grant => grants.Contains(grant.GrantType)));

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x => x.ClientId.Contains(filter) || x.ClientName.Contains(filter));
        }

        IQueryable<ClientSummaryModel> result = query.Select(x => new ClientSummaryModel
        {
            ClientId = x.ClientId,
            Name = x.ClientName,
            Flow = x.AllowedGrantTypes.Select(x => x.GrantType).Single() == GrantType.ClientCredentials ? Flow.ClientCredentials : Flow.CodeFlowWithPkce
        });

        return await result.ToArrayAsync();
    }

    public async Task<ClientModel?> GetByIdAsync(string id)
    {
        Client? client = await _context.Clients
            .Include(x => x.AllowedGrantTypes)
            .Include(x => x.AllowedScopes)
            .Include(x => x.RedirectUris)
            .Include(x => x.PostLogoutRedirectUris)
            .Where(x => x.ClientId == id)
            .SingleOrDefaultAsync();

        if (client == null)
        {
            return null;
        }

        return new ClientModel
        {
            ClientId = client.ClientId,
            Name = client.ClientName,
            Flow = client.AllowedGrantTypes.Select(x => x.GrantType).Single() == GrantType.ClientCredentials ? Flow.ClientCredentials : Flow.CodeFlowWithPkce,
            AllowedScopes = client.AllowedScopes.Count != 0 ? client.AllowedScopes.Select(x => x.Scope).Aggregate((a, b) => $"{a} {b}") : string.Empty,
            RedirectUri = client.RedirectUris.Select(x => x.RedirectUri).FirstOrDefault(),
            InitiateLoginUri = client.InitiateLoginUri,
            PostLogoutRedirectUri = client.PostLogoutRedirectUris.Select(x => x.PostLogoutRedirectUri).FirstOrDefault(),
            FrontChannelLogoutUri = client.FrontChannelLogoutUri,
            BackChannelLogoutUri = client.BackChannelLogoutUri,
        };
    }

    public async Task CreateAsync(CreateClientModel model)
    {
        ArgumentNullException.ThrowIfNull(model);   
        var client = new Duende.IdentityServer.Models.Client
        {
            ClientId = model.ClientId.Trim(),
            ClientName = model.Name?.Trim()
        };

        client.ClientSecrets.Add(new Duende.IdentityServer.Models.Secret(model.Secret.Sha256()));
        
        if (model.Flow == Flow.ClientCredentials)
        {
            client.AllowedGrantTypes = GrantTypes.ClientCredentials;
        }
        else
        {
            client.AllowedGrantTypes = GrantTypes.Code;
            client.AllowOfflineAccess = true;
        }

#pragma warning disable CA1849 // Call async methods when in an async method
// CA1849 Suppressed because AddAsync is only needed for value generators that
// need async database access (e.g., HiLoValueGenerator), and we don't use those
// generators
        _context.Clients.Add(client.ToEntity());
#pragma warning restore CA1849 // Call async methods when in an async method
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClientModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Client? client = await _context.Clients
            .Include(x => x.AllowedGrantTypes)
            .Include(x => x.AllowedScopes)
            .Include(x => x.RedirectUris)
            .Include(x => x.PostLogoutRedirectUris)
            .SingleOrDefaultAsync(x => x.ClientId == model.ClientId);

        if (client == null)
        {
            throw new ArgumentException("Invalid Client Id");
        }

        if (client.ClientName != model.Name)
        {
            client.ClientName = model.Name?.Trim();
        }

        string[] scopes = model.AllowedScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
        string[] currentScopes = client.AllowedScopes.Select(x => x.Scope).ToArray();

        string[] scopesToAdd = scopes.Except(currentScopes).ToArray();
        string[] scopesToRemove = currentScopes.Except(scopes).ToArray();

        if (scopesToRemove.Length != 0)
        {
            client.AllowedScopes.RemoveAll(x => scopesToRemove.Contains(x.Scope));
        }
        if (scopesToAdd.Length != 0)
        {
            client.AllowedScopes.AddRange(scopesToAdd.Select(x => new ClientScope
            {
                Scope = x,
            }));
        }

        Flow flow = client.AllowedGrantTypes.Select(x => x.GrantType)
            .Single() == GrantType.ClientCredentials ? Flow.ClientCredentials : Flow.CodeFlowWithPkce;

        if (flow == Flow.CodeFlowWithPkce)
        {
            if (client.RedirectUris.SingleOrDefault()?.RedirectUri != model.RedirectUri)
            {
                client.RedirectUris.Clear();
                if (model.RedirectUri != null)
                {
                    client.RedirectUris.Add(new ClientRedirectUri { RedirectUri = model.RedirectUri.Trim() });
                }
            }
            if (client.InitiateLoginUri != model.InitiateLoginUri)
            {
                client.InitiateLoginUri = model.InitiateLoginUri;
            }
            if (client.PostLogoutRedirectUris.SingleOrDefault()?.PostLogoutRedirectUri != model.PostLogoutRedirectUri)
            {
                client.PostLogoutRedirectUris.Clear();
                if (model.PostLogoutRedirectUri != null)
                {
                    client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = model.PostLogoutRedirectUri.Trim() });
                }
            }
            if (client.FrontChannelLogoutUri != model.FrontChannelLogoutUri)
            {
                client.FrontChannelLogoutUri = model.FrontChannelLogoutUri?.Trim();
            }
            if (client.BackChannelLogoutUri != model.BackChannelLogoutUri)
            {
                client.BackChannelLogoutUri = model.BackChannelLogoutUri?.Trim();
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string clientId)
    {
        Client? client = await _context.Clients.SingleOrDefaultAsync(x => x.ClientId == clientId);

        if (client == null)
        {
            throw new ArgumentException("Invalid Client Id");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }
}
