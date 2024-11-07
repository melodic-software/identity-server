using Enterprise.DI.Core.Registration.Abstract;
using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Configuration.Ciba;
using IdentityServer.Configuration.Constants;
using IdentityServer.Configuration.Services;
using IdentityServer.Configuration.Services.UserInteraction;
using IdentityServer.Constants;
using IdentityServer.Pages;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Configuration;

internal sealed class IdentityServerServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringKeys.IdentityServer);

        IIdentityServerBuilder idsvrBuilder = services
            .AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v7/reference/options/

                options.Authentication.CookieAuthenticationScheme = CookieSchemes.CookieAuthenticationScheme;

                // https://docs.duendesoftware.com/identityserver/v7/ui/logout/session_cleanup/
                // This will remove long-lived tokens (refresh tokens) associated with the user when they log out.
                options.Authentication.CoordinateClientLifetimesWithUserSession = true;

                options.EmitScopesAsSpaceDelimitedStringInJwt = true;

                // Emits a static claim with the format {issuer}/resources.
                // Example: https://identity.example.com/resources
                // NOTE: Both the static audience and audiences from API resources can be used.
                options.EmitStaticAudienceClaim = true;

                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;

                // https://docs.duendesoftware.com/identityserver/v7/fundamentals/keys/automatic_key_management
                options.KeyManagement.Enabled = true;
                options.KeyManagement.RotationInterval = TimeSpan.FromDays(90);
                options.KeyManagement.PropagationTime = TimeSpan.FromDays(14);
                options.KeyManagement.RetentionDuration = TimeSpan.FromDays(14);
                options.KeyManagement.DeleteRetiredKeys = true;

                // TODO: Add this when ready to go live in production.
                // We will need a community license from Duende (send email).
                //options.LicenseKey = "";

                if (environment.IsDevelopment())
                {
                    // Claim type used for the user’s display name. Unset by default due to possible PII concerns.
                    // If used, this would commonly be JwtClaimTypes.Name, JwtClaimType.Email or a custom claim.
                    options.ServerSideSessions.UserDisplayNameClaimType = JwtClaimTypes.Email;

                    // https://docs.duendesoftware.com/identityserver/v7/ui/server_side_sessions/session_expiration/
                    options.ServerSideSessions.RemoveExpiredSessionsFrequency = TimeSpan.FromMinutes(10);

                    // https://docs.duendesoftware.com/identityserver/v7/ui/server_side_sessions/inactivity_timeout/
                    options.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout = true;
                }

                options.UserInteraction.LoginUrl = PathConstants.LoginPath;
                options.UserInteraction.LogoutUrl = PathConstants.LogoutPath;
                options.UserInteraction.ConsentUrl = PageConstants.Consent;
                options.UserInteraction.ErrorUrl = PageConstants.Error;
                options.UserInteraction.DeviceVerificationUrl = PageConstants.DeviceVerification;
                options.UserInteraction.CreateAccountUrl = PathConstants.RegisterPath;

                // https://github.com/DuendeSoftware/IdentityServer/releases/tag/7.0.7
                //options.UserInteraction.PromptValuesSupported.Add("custom-prompt");
            })
            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryClients(Config.Clients)
            // This adds the config data from DB (clients, resources, CORS).
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        dbOpts =>
                        {
                            dbOpts.MigrationsAssembly(AssemblyReference.Assembly.FullName);
                            dbOpts.EnableRetryOnFailure();
                        });
            })
            // https://docs.duendesoftware.com/identityserver/v7/data/ef/#enabling-caching-for-configuration-store
            // This should be enabled in production to reduce database load (request volume).
            // TODO: It might be better to use a distributed cache instead of in-memory.
            .AddConfigurationStoreCache()
            // This adds the operational data from DB (codes, tokens, consents).
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        dbOpts =>
                        {
                            dbOpts.MigrationsAssembly(AssemblyReference.Assembly.FullName);
                            dbOpts.EnableRetryOnFailure();
                        });

                // This enables automatic token cleanup (optional).
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600; // Cleanup expired tokens once per hour.
            })
            //.AddTestUsers(TestUsers.Users)
            .AddAspNetIdentity<ApplicationUser>()
            .AddAuthorizeInteractionResponseGenerator<CustomAuthorizeInteractionResponseGenerator>()
            .AddProfileService<CustomProfileService>()
            .AddBackchannelAuthenticationUserValidator<BackChannelAuthenticationUserValidator>()
            .AddBackchannelAuthenticationUserNotificationService<BackchannelAuthenticationUserNotificationService>();

        // https://blog.duendesoftware.com/posts/20220406_session_management/
        // https://docs.duendesoftware.com/identityserver/v7/ui/server_side_sessions/
        idsvrBuilder.AddServerSideSessions();

        // https://docs.duendesoftware.com/identityserver/v7/ui/login/external/
        // Configures the OpenIdConnect handlers to persist the state parameter into the server-side IDistributedCache.
        // This requires that an implementation of IDistributedCache has been registered.
        services.AddOidcStateDataFormatterCache();
    }
}
