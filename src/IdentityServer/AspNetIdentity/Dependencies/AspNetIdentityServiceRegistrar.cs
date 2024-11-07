using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.AspNetIdentity.Constants;
using IdentityServer.AspNetIdentity.Dependencies.Extensions;
using IdentityServer.AspNetIdentity.EntityFramework;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.AspNetIdentity.Passwords.Validators;
using IdentityServer.Security.Authentication.Cookies.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServer.AspNetIdentity.Dependencies;

// https://learn.microsoft.com/en-us/aspnet/identity/overview
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0

internal sealed class AspNetIdentityServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringKeys.AspNetIdentity);

        services.AddDbContext<AspNetIdentityDbContext>(options =>
            options.UseSqlServer(connectionString, optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly(AssemblyReference.Assembly.GetName().FullName);
                optionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Dbo);
                optionsBuilder.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
            }));

        // This does not register the default UI.
        IdentityBuilder identityBuilder = services
            .AddIdentity<ApplicationUser, ApplicationRole>()

            // Add a custom user store if the user entity additional related entities need to be loaded
            // or claims based off those relationships should be included.
            // Here is a demo use case: https://app.pluralsight.com/ilx/video-courses/77f2d072-e8ab-4806-9310-dcc770bc1ce0/5d205b40-8f89-4916-ab85-40955adedad5/45fba6ea-8dca-4605-a936-fd18a93dd001

            //.AddApiEndpoints()
            .AddEntityFrameworkStores(services)
            .AddDefaultTokenProviders()
            .AddPasswordValidator<CustomPasswordValidator>();

        services.ConfigureIdentityOptions()
            .ConfigureCookies(configuration, environment)
            .Configure<DataProtectionTokenProviderOptions>(o =>
            {
                // This sets the global token lifespan for email confirmation, password resets, and 2FA codes.
                // TODO: Add configuration or reference these options in the emails so the email templates are always accurate.
                // If separate values are needed for specific tokens, a custom token provider will need to be created.
                o.TokenLifespan = TimeSpan.FromHours(24);
            })
            .ConfigurePasswordHashing(configuration)
            .Configure<SecurityStampValidatorOptions>(o =>
            {
                // Security stamps are security stamps are re-validated every 30 minutes.
                o.ValidationInterval = TimeSpan.FromMinutes(30);
            });
    }
}