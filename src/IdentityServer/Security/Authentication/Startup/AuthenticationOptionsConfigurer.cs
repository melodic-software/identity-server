using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;
using IdentityServer.Constants;
using IdentityServer.Security.Authentication.External.Extensions;

namespace IdentityServer.Security.Authentication.Startup;

internal sealed class AuthenticationOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureAuthentication(authenticationOptions =>
        {
            authenticationOptions.ConfigureOptions += o =>
            {
                o.DefaultScheme = CookieSchemes.CookieAuthenticationScheme;
                o.DefaultChallengeScheme = CookieSchemes.CookieAuthenticationScheme;
            };

            authenticationOptions.ConfigureBuilder += builder =>
            {
                builder.AddGoogle(configuration)
                    .AddMicrosoft(configuration)
                    .AddSpotify(configuration)
                    .AddDemoIdentityServer(configuration, environment);
            };

            // We don't use the built-in authentication middleware directly.
            // Our identity server middleware takes care of it.
            // We're essentially swapping the call.

            authenticationOptions.UseAuthentication = app =>
            {
                app.UseIdentityServer();
            };
        });
    }
}
