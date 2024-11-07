using Enterprise.Applications.AspNetCore.Security.Authentication.JwtBearer.Options;
using Enterprise.Applications.AspNetCore.Security.Authorization.Options;
using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Options.Core.Startup;
using IdentityModel;
using IdentityServer.Constants;
using IdentityServer.Security.Authorization.Policies;
using IdentityServer.Security.Authorization.Roles.Constants;

namespace IdentityServer.Security.Authorization.Startup;

internal sealed class AuthorizationOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        JwtBearerTokenOptions jwtBearerTokenOptions = StartupOptionsService.Instance.Get<JwtBearerTokenOptions>(configuration);

        options.Configure<AuthorizationOptions>(authorizationOptions =>
        {
            authorizationOptions.ConfigureBuilder += builder =>
            {
                builder.AddPolicy(PolicyNames.Admin, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Role, RoleNames.IdentityServerAdmin);
                    policy.AddAuthenticationSchemes(CookieSchemes.CookieAuthenticationScheme);
                });

                builder.AddPolicy(PolicyNames.ApiAccess, policy =>
                {
                    List<string> apiAccessSchemes = [];

                    if (jwtBearerTokenOptions.EnableJwtBearerTokens)
                    {
                        // Prioritize JWT bearer token authentication first.
                        apiAccessSchemes.Add(jwtBearerTokenOptions.AuthenticationScheme);
                    }

                    // If they are authenticated with a cookie, this can also be used as a secondary auth scheme.
                    apiAccessSchemes.Add(CookieSchemes.CookieAuthenticationScheme);

                    policy.RequireAuthenticatedUser();
                    policy.AddAuthenticationSchemes(apiAccessSchemes.ToArray());
                });
            };
        });
    }
}
