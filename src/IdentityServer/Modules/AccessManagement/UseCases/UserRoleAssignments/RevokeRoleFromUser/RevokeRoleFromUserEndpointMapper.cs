using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.AccessManagement.UseCases.Shared;
using IdentityServer.Modules.AccessManagement.UseCases.UserRoleAssignments.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.AccessManagement.UseCases.UserRoleAssignments.RevokeRoleFromUser;

public class RevokeRoleFromUserEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"{RoutePrefixes.AccessManagement}/{ResourceNames.UserRoleAssignments}", RevokeRoleFromUser)
            .WithName(RouteNames.RevokeUserRole)
            .WithDisplayName("Revoke Role from User")
            .WithDescription("Revokes a specific role from a user, removing their access or permissions associated with that role.")
            .WithTags(Tags.UserRoleAssignments)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    public static async Task<IResult> RevokeRoleFromUser(string roleName, string userId, UserManager<ApplicationUser> userManager, HttpContext httpContext)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return ErrorResultFactory.ToResult([RoleAssignmentErrors.UserNotFound], httpContext);
        }

        IdentityResult result = await userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return TypedResults.NoContent();
        }

        var validationErrors = result.Errors
            .Select(x => Error.Validation(x.Code, x.Description))
            .ToList();

        return validationErrors.ToResult(httpContext);
    }
}