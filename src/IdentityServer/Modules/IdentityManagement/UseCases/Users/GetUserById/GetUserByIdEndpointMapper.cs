using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Shared;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;

public class GetUserByIdEndpointMapper : IMapEndpoint
{
    private readonly IConfiguration _configuration;

    public GetUserByIdEndpointMapper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet($"{RoutePrefixes.IdentityManagement}/{ResourceNames.Users}/{{userId}}", GetUserById)
            .WithName(RouteNames.GetUserById)
            .WithDisplayName("Get User By Id")
            .WithDescription("This endpoint returns a user whose identifier (GUID) matches the provided value (if exists).")
            .WithTags(Tags.Users)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi(x =>
            {
                OpenApiParameter userId = x.Parameters.First(p => p.Name == "userId");
                string? adminUserId = _configuration.GetValue<string>(ConfigurationKeys.AdminUserId);
                userId.Example = new OpenApiString(adminUserId);
                return x;
            });
    }

    /// <summary>
    /// Get a user by a unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of a user.</param>
    /// <param name="queryDispatcher"></param>
    /// <param name="httpContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<IResult> GetUserById(string userId,
        IQueryDispatchFacade queryDispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);

        Result<User> result = await queryDispatcher.DispatchAsync(query, cancellationToken);

        return result.Match(queryResult =>
            {
                var response = new GetUserByIdResponse
                {
                    UserId = queryResult.UserId,
                    Email = queryResult.Email,
                    FirstName = queryResult.FirstName,
                    LastName = queryResult.LastName
                };

                return TypedResults.Ok(response);
            },
            errors => ErrorResultFactory.ToResult(errors.ToList(), httpContext)
        );
    }
}