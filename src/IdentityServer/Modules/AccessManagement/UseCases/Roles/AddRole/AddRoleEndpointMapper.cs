using Enterprise.Applications.AspNetCore.Api.Minimal.Mapping;
using Enterprise.Applications.AspNetCore.Api.Minimal.RouteHandling;
using Enterprise.Applications.AspNetCore.ErrorHandling.Domain;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.AccessManagement.UseCases.Shared;
using IdentityServer.Security.Authorization.Policies;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.AccessManagement.UseCases.Roles.AddRole;

public class AddRoleEndpointMapper : IMapEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{RoutePrefixes.AccessManagement}/{ResourceNames.Roles}", AddRole)
            .WithName(RouteNames.AddRole)
            .WithDisplayName("Add Role")
            .WithDescription("This endpoint allows for the creation of a new role in the system.")
            .WithTags(Tags.Roles)
            .ProducesStandard()
            .RequireAuthorization(PolicyNames.ApiAccess)
            .WithOpenApi();
    }

    public static async Task<IResult> AddRole(string roleName, RoleManager<ApplicationRole> roleManager, HttpContext httpContext)
    {
        ApplicationRole? existingRole = await roleManager.FindByNameAsync(roleName);

        if (existingRole != null)
        {
            return TypedResults.Ok();
        }

        var role = new ApplicationRole
        {
            Name = roleName
        };

        IdentityResult result = await roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            return TypedResults.CreatedAtRoute(RouteNames.GetRoleByName, new { name = role.Name });
        }

        var validationErrors = result.Errors
            .Select(x => Error.Validation(x.Code, x.Description))
            .ToList();

        return validationErrors.ToResult(httpContext);
    }
}