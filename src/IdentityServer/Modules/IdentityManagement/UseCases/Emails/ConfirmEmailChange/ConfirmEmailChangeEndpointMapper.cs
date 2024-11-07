using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.Modules.IdentityManagement.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

public class ConfirmEmailChangeEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost($"/{RoutePrefixes.IdentityManagement}/{ResourceNames.EmailChangeConfirmations}", ConfirmEmailChange)
            .WithName(RouteNames.ConfirmEmailChange)
            .WithDisplayName("Confirm Email Change")
            .WithDescription("This endpoint is for confirming the email address change for a user.")
            .WithSummary("This endpoint is for confirming the email address change for a user.")
            .WithTags(Tags.EmailChangeConfirmations)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    private static async Task<IResult> ConfirmEmailChange(string userId, string newEmail, string token,
        ICommandDispatchFacade commandDispatcher, HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var confirmEmailChangeCommand = new ConfirmEmailChangeCommand(userId, newEmail, token);
        Result confirmEmailChangeResult = await commandDispatcher.DispatchAsync(confirmEmailChangeCommand, cancellationToken);

        return confirmEmailChangeResult.HasErrors
            ? ErrorResultFactory.ToResult(confirmEmailChangeResult.Errors, httpContext)
            : TypedResults.Created();
    }
}