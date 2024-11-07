using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.Modules.IdentityManagement.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;

public class ResetPasswordEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost($"/{RoutePrefixes.IdentityManagement}/{ResourceNames.PasswordResets}", ResetPassword)
            .WithName(RouteNames.ResetPassword)
            .WithDisplayName("Reset Password")
            .WithDescription("This endpoint is for resetting the password for a specific user.")
            .WithSummary("This endpoint is for resetting the password for a specific user.")
            .WithTags(Tags.PasswordResets)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    private static async Task<IResult> ResetPassword(
        ResetPasswordRequest request,
        ICommandDispatchFacade commandDispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new ResetPasswordCommand(request.UserId, request.PasswordResetToken, request.NewPassword);
        Result result = await commandDispatcher.DispatchAsync(command, cancellationToken);
        return result.Match(Results.Created, errors => errors.ToResult(httpContext));
    }
}