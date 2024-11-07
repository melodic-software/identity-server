using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using IdentityServer.Modules.OAuth.UseCases.ApiResources.Shared;
using IdentityServer.Modules.OAuth.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Modules.OAuth.UseCases.ApiResources.GetApiResourceByName;

public class GetApiResourceByNameEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet($"/{RoutePrefixes.OAuth}/{ResourceNames.ApiResources}", GetApiResourceByName)
            .WithName(RouteNames.GetApiResourceByName)
            .WithDisplayName("Get API Resource By Name")
            .WithDescription("Get an API resource by name.")
            .WithSummary("This endpoint is for retrieving an API resource by name.")
            .WithTags(Tags.ApiResources)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi(x => x);
    }

    private static async Task<IResult> GetApiResourceByName(string name,
        ConfigurationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ApiResource? apiResource = await dbContext.ApiResources
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        if (apiResource == null)
        {
            return TypedResults.NotFound();
        }

        var response = new ApiResourceResponse
        {
            Name = apiResource.Name,
            DisplayName = apiResource.DisplayName,
            Description = apiResource.Description,
            Enabled = apiResource.Enabled,
            AllowedAccessTokenSigningAlgorithms = apiResource.AllowedAccessTokenSigningAlgorithms,
            ShowInDiscoveryDocument = apiResource.ShowInDiscoveryDocument,
            RequireResourceIndicator = apiResource.RequireResourceIndicator,
            DateCreated = apiResource.Created,
            DateUpdated = apiResource.Updated,
            DateLastAccessed = apiResource.LastAccessed
        };

        return TypedResults.Ok(response);
    }
}