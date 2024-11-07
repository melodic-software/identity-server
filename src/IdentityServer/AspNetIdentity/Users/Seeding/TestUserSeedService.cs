using Duende.IdentityServer;
using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;

namespace IdentityServer.AspNetIdentity.Users.Seeding;

public static class TestUserSeedService
{
    public static async Task SeedTestUsersAsync(IServiceScope scope, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(nameof(TestUserSeedService));

        UserManager<ApplicationUser> userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var address = new
        {
            street_address = "One Hacker Way",
            locality = "Heidelberg",
            postal_code = "69118",
            country = "Germany"
        };

        ApplicationUser? alice = await userMgr.FindByNameAsync("alice");

        if (alice == null)
        {
            alice = new()
            {
                Id = "a5a0c16c-0027-4011-8e2f-a7e179dc784b",
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
                DateEmailConfirmed = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow
            };

            IdentityResult result = await userMgr.CreateAsync(alice, "Pass123$");

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(alice, [
                new(JwtClaimTypes.Name, "Alice Smith"),
                new(JwtClaimTypes.GivenName, "Alice"),
                new(JwtClaimTypes.FamilyName, "Smith"),
                new(JwtClaimTypes.Email, "AliceSmith@email.com"),
                new(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new(JwtClaimTypes.WebSite, "http://alice.com"),
                new(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
            ]);

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            logger.LogDebug("alice created");
        }
        else
        {
            logger.LogDebug("alice already exists");
        }

        ApplicationUser? bob = await userMgr.FindByNameAsync("bob");

        if (bob == null)
        {
            bob = new()
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true,
                DateEmailConfirmed = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow
            };

            IdentityResult result = await userMgr.CreateAsync(bob, "Pass123$");

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(bob, [
                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                new Claim(JwtClaimTypes.GivenName, "Bob"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json),
                new("location", "somewhere")
            ]);

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            logger.LogDebug("bob created");
        }
        else
        {
            logger.LogDebug("bob already exists");
        }

        ApplicationUser? admin = await userMgr.FindByNameAsync("admin");

        if (admin == null)
        {
            admin = new()
            {
                Id = "650c4cec-f4d2-4023-9f43-79047011f124",
                UserName = "admin",
                Email = "admin@melodicsoftware.com",
                EmailConfirmed = true,
                DateEmailConfirmed = DateTime.UtcNow,
                LockoutEnabled = false,
                DateCreated = DateTime.UtcNow
            };

            IdentityResult result = await userMgr.CreateAsync(admin, "Pass123$");

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(admin, []);

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
    }
}
