using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;
using IdentityServer.Security.Authorization.Policies;

namespace IdentityServer.Pages;

internal sealed class RazorPagesOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureRazorPages(razorPagesOptions =>
        {
            razorPagesOptions.ConfigureOptions += o =>
            {
                o.Conventions.AuthorizeFolder("/Admin", PolicyNames.Admin);
                o.Conventions.AuthorizeFolder("/ServerSideSessions", PolicyNames.Admin);
            };
        });
    }
}
