using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using IdentityServer.Modules.OAuth.UseCases.ApiResources.Shared;
using IdentityServer.Modules.OAuth.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Modules.OAuth.UseCases.ApiResources.CreateApiResource;

public class CreateApiResourceEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost($"/{RoutePrefixes.OAuth}/{ResourceNames.ApiResources}", CreateApiResource)
            .WithName(RouteNames.CreateApiResource)
            .WithDisplayName("Create API Resource")
            .WithDescription("This endpoint is for creating new API resources.")
            .WithSummary("This endpoint is for creating new API resources.")
            .WithTags(Tags.ApiResources)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi(x => x);
    }

    private static async Task<IResult> CreateApiResource(CreateApiResourceRequest request,
        ConfigurationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ApiResource? apiResource = await dbContext.ApiResources
            .FirstOrDefaultAsync(x => x.Name == request.Name, cancellationToken);

        if (apiResource != null)
        {
            return TypedResults.CreatedAtRoute(RouteNames.GetApiResourceByName, new { name = apiResource.Name });
        }

        apiResource = new ApiResource
        {
            Enabled = true,
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            AllowedAccessTokenSigningAlgorithms = null,
            ShowInDiscoveryDocument = true,
            RequireResourceIndicator = false,
            Created = DateTime.UtcNow,
            Updated = null,
            LastAccessed = null,
            NonEditable = false
        };

        await dbContext.AddAsync(apiResource, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(RouteNames.GetApiResourceByName, new { name = apiResource.Name });
    }
}