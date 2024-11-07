using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.Modules.IdentityManagement.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.SendPasswordResetEmail;

public class SendPasswordResetEmailEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost($"/{RoutePrefixes.IdentityManagement}/{ResourceNames.PasswordResetEmails}", SendPasswordResetEmail)
            .WithName(RouteNames.SendPasswordResetEmail)
            .WithDisplayName("Send Password Reset Email")
            .WithDescription("This endpoint is for sending password resets emails for a specific user.")
            .WithSummary("This endpoint is for sending password resets emails for a specific user.")
            .WithTags(Tags.PasswordResetEmails)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    private static async Task<IResult> SendPasswordResetEmail(
        string userId,
        ICommandDispatchFacade commandDispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new SendPasswordResetEmailCommand(userId);
        Result result = await commandDispatcher.DispatchAsync(command, cancellationToken);
        return result.Match(Results.Created, errors => errors.ToResult(httpContext));
    }
}
