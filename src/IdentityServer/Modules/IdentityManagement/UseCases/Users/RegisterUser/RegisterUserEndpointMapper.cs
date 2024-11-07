using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public class RegisterUserEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost($"/{RoutePrefixes.IdentityManagement}/{ResourceNames.UserRegistrations}", RegisterUser)
            .WithName(RouteNames.RegisterUser)
            .WithDisplayName("Register User")
            .WithDescription("This endpoint is for registering new users with the system.")
            .WithSummary("This endpoint is for registering new users with the system.")
            .WithTags(Tags.UserRegistrations)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        ICommandDispatchFacade commandDispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(
            request.UserId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.IsExternalLogin,
            request.ExternalProvider,
            request.ExternalUserId,
            request.ReturnUrl
        );

        Result<string> result = await commandDispatcher.DispatchAsync(command, cancellationToken);

        return result.Match(
            userId => Results.CreatedAtRoute(RouteNames.GetUserById, new { userId }),
            errors => ErrorResultFactory.ToResult(errors.ToList(), httpContext)
        );
    }
}