using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using IdentityServer.Modules.OAuth.UseCases.ApiResources.Shared;
using IdentityServer.Modules.OAuth.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Modules.OAuth.UseCases.ApiScopes.GetApiScopeByName;

public class GetApiScopeByNameEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost($"/{ResourceNames.ApiScopes}", GetApiScopeByName)
            .WithName(RouteNames.GetApiScopeByName)
            .WithDisplayName("Get API Resource By Name")
            .WithDescription("Get an API resource by name.")
            .WithSummary("This endpoint is for retrieving an API scope by name.")
            .WithTags(Tags.ApiScopes)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi(x => x);
    }

    public static async Task<IResult> GetApiScopeByName(string name, ConfigurationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ApiScope? apiScope = await dbContext.ApiScopes
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        if (apiScope == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(apiScope);
    }
}