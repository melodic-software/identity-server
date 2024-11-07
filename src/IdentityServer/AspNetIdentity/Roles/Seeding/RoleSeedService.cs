using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Security.Authorization.Roles.Constants;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.Roles.Seeding;

public static class RoleSeedService
{
    public static async Task SeedRolesAsync(IServiceScope scope)
    {
        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(nameof(RoleSeedService));

        RoleManager<ApplicationRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        ApplicationRole? identityServerAdminRole = await roleManager.FindByNameAsync(RoleNames.IdentityServerAdmin);

        if (identityServerAdminRole == null)
        {
            identityServerAdminRole = new ApplicationRole
            {
                Name = RoleNames.IdentityServerAdmin
            };

            IdentityResult result = await roleManager.CreateAsync(identityServerAdminRole);
        }
    }
}