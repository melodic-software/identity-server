using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Security.Authorization.Roles.Constants;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.AspNetIdentity.Users.Seeding;

public static class UserSeedService
{
    public static async Task SeedAdminUserAsync(IServiceScope scope, IConfiguration configuration)
    {
        // TODO: Add configuration around this.

        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(nameof(UserSeedService));

        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string? adminUserId = configuration.GetValue<string>(ConfigurationKeys.AdminUserId);

        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            throw new InvalidOperationException("An admin user ID has not been configured.");
        }

        string? adminEmail = configuration.GetValue<string>(ConfigurationKeys.AdminUserEmail);

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            throw new InvalidOperationException("An admin email has not been configured.");
        }

        ApplicationUser? admin = await userManager.FindByIdAsync(adminUserId);

        if (admin == null)
        {
            admin = new()
            {
                Id = adminUserId,
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DateEmailConfirmed = DateTime.UtcNow,
                LockoutEnabled = false,
                DateCreated = DateTime.UtcNow
            };

            // This will need to be immediately changed after being seeded.
            IdentityResult result = await userManager.CreateAsync(admin, "Pass123$");

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            string firstName = configuration.GetValue<string>(ConfigurationKeys.AdminUserFirstName);
            string lastName = configuration.GetValue<string>(ConfigurationKeys.AdminUserLastName);

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new InvalidOperationException("The first name of the admin has not been configured.");
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new InvalidOperationException("The last name of the admin has not been configured.");
            }

            var claims = new List<Claim>
            {
                new(JwtClaimTypes.Name, $"{firstName} {lastName}"),
                new(JwtClaimTypes.GivenName, firstName),
                new(JwtClaimTypes.FamilyName, lastName)
            };

            result = await userManager.AddClaimsAsync(admin, claims);

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            logger.LogDebug("admin created");
        }
        else
        {
            logger.LogDebug("admin already exists");
        }

        bool userIsInAdminRole = await userManager.IsInRoleAsync(admin, RoleNames.IdentityServerAdmin);

        if (!userIsInAdminRole)
        {
            IdentityResult result = await userManager.AddToRoleAsync(admin, RoleNames.IdentityServerAdmin);
        }
    }
}
