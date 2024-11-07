using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Pages.Admin.ApiScopes;

public class ApiScopeRepository
{
    private readonly ConfigurationDbContext _context;

    public ApiScopeRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApiScopeSummaryModel>, int)> GetAllAsync(string? filter = null, int pageNumber = 1, int pageSize = 10)
    {
        IQueryable<ApiScope> query = _context.ApiScopes
            .Include(x => x.UserClaims)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x => x.Name.Contains(filter) || x.DisplayName.Contains(filter));
        }

        int totalCount = await query.CountAsync();

        ApiScopeSummaryModel[] scopes = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ApiScopeSummaryModel
            {
                ApiScopeId = x.Id,
                Name = x.Name,
                Description = x.Description,
                DisplayName = x.DisplayName,
                IsEnabled = x.Enabled
            })
            .ToArrayAsync();

        return (scopes, totalCount);
    }

    public async Task<ApiScopeModel?> GetByIdAsync(string id)
    {
        ApiScope? scope = await _context.ApiScopes
            .Include(x => x.UserClaims)
            .SingleOrDefaultAsync(x => x.Name == id);

        if (scope == null)
        {
            return null;
        }

        return new ApiScopeModel
        {
            ApiScopeId = scope.Id,
            Name = scope.Name,
            DisplayName = scope.DisplayName,
            Description = scope.Description,
            IsEnabled = scope.Enabled,
            Required = scope.Required,
            Emphasize = scope.Emphasize,
            ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
            IsNonEditable = scope.NonEditable,
            UserClaims = scope.UserClaims.Count != 0
                ? scope.UserClaims.Select(x => x.Type).Aggregate((a, b) => $"{a} {b}")
                : null,
        };
    }

    public async Task CreateAsync(ApiScopeModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var scope = new Duende.IdentityServer.Models.ApiScope
        {
            Name = model.Name,
            DisplayName = model.DisplayName?.Trim(),
            Description = model.Description?.Trim(),
            Enabled = model.IsEnabled,
            Required = model.Required,
            Emphasize = model.Emphasize,
            ShowInDiscoveryDocument = model.ShowInDiscoveryDocument
        };

        IEnumerable<string> claims = model.UserClaims?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray() ?? [];
        
        if (claims.Any())
        {
            scope.UserClaims = claims.ToList();
        }

#pragma warning disable CA1849 // Call async methods when in an async method
// CA1849 Suppressed because AddAsync is only needed for value generators that
// need async database access (e.g., HiLoValueGenerator), and we don't use those
// generators
        _context.ApiScopes.Add(scope.ToEntity());
#pragma warning restore CA1849 // Call async methods when in an async method
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApiScopeModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ApiScope? scope = await _context.ApiScopes
            .Include(x => x.UserClaims)
            .SingleOrDefaultAsync(x => x.Name == model.Name);

        if (scope == null)
        {
            throw new ArgumentException("Invalid Api Scope");
        }

        if (scope.DisplayName != model.DisplayName)
        {
            scope.DisplayName = model.DisplayName?.Trim();
        }

        if (scope.Description != model.Description)
        {
            scope.Description = model.Description?.Trim();
        }

        scope.Enabled = model.IsEnabled;
        scope.Required = model.Required;
        scope.Emphasize = model.Emphasize;
        scope.ShowInDiscoveryDocument = model.ShowInDiscoveryDocument;
        scope.NonEditable = model.IsNonEditable;

        IEnumerable<string> claims = model.UserClaims?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray() ?? [];
        string[] currentClaims = scope.UserClaims.Select(x => x.Type).ToArray();

        string[] claimsToAdd = claims.Except(currentClaims).ToArray();
        string[] claimsToRemove = currentClaims.Except(claims).ToArray();

        if (claimsToRemove.Length != 0)
        {
            scope.UserClaims.RemoveAll(x => claimsToRemove.Contains(x.Type));
        }
        if (claimsToAdd.Length != 0)
        {
            scope.UserClaims.AddRange(claimsToAdd.Select(x => new ApiScopeClaim
            {
                Type = x,
            }));
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        ApiScope? scope = await _context.ApiScopes.SingleOrDefaultAsync(x => x.Name == id);

        if (scope == null)
        {
            throw new ArgumentException("Invalid Api Scope");
        }

        _context.ApiScopes.Remove(scope);

        await _context.SaveChangesAsync();
    }
}
