using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.AccessManagement.UseCases.Shared;
using IdentityServer.Modules.AccessManagement.UseCases.UserRoleAssignments.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.AccessManagement.UseCases.UserRoleAssignments.AssignRoleToUser;

public class AssignRoleToUserEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{RoutePrefixes.AccessManagement}/{ResourceNames.UserRoleAssignments}", AssignRoleToUser)
            .WithName(RouteNames.AssignRoleToUser)
            .WithDisplayName("Assign Role to User")
            .WithDescription("This endpoint allows for the assignment of a role to a user.")
            .WithTags(Tags.UserRoleAssignments)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    public static async Task<IResult> AssignRoleToUser(string roleName, string userId, UserManager<ApplicationUser> userManager, HttpContext httpContext)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return ErrorResultFactory.ToResult([RoleAssignmentErrors.UserNotFound], httpContext);
        }

        IdentityResult result = await userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return TypedResults.Created();
        }

        var validationErrors = result.Errors
            .Select(x => Error.Validation(x.Code, x.Description))
            .ToList();

        return validationErrors.ToResult(httpContext);
    }
}