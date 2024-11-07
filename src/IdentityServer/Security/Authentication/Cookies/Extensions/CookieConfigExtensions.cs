using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using Enterprise.Applications.AspNetCore.Security.Https.Options;
using Enterprise.Logging.Core.Startup;
using Enterprise.Options.Core.Startup;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Pages;
using IdentityServer.Security.Claims.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Security.Authentication.Cookies.Extensions;

public static class CookieConfigExtensions
{
    private static ILogger Logger => StartupLogger.Instance;

    public static IServiceCollection ConfigureCookies(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // https://docs.duendesoftware.com/identityserver/v7/ui/login/
        // This specifically configures the application cookie used by ASP.NET Core Identity.
        // NOTE: This does not affect the other cookie handlers and options for both ASP.NET Core Identity and Identity Server.
        // The others include Identity.External, Identity.TwoFactorRememberMe, Identity.TwoFactorUserId, idsrv and idsrv.external.

        HttpsOptions httpsOptions = StartupOptionsService.Instance.Get<HttpsOptions>(configuration);

        // This is also known as the "identity cookie" since it is the primary cookie
        // containing the user's identity in the form of claims.
        services.ConfigureApplicationCookie(o =>
        {
            o.LoginPath = PathConstants.LoginPath;
            o.LogoutPath = PathConstants.LogoutPath;

            o.AccessDeniedPath = PathConstants.AccessDeniedPath;

            // Complete customization can be added here
            //o.Events.OnRedirectToAccessDenied = context =>
            //{
            //    return Task.CompletedTask;
            //};

            o.ReturnUrlParameter = ParameterNames.ReturnUrl;

            // Strict means that a cookie will never be sent cross site from one site to another.
            // Lax introduces one exception to that rule. Only hyperlinks in the HTML of the other site will work and nothing else.
            // None disabled same site cookies (never do this).
            // If no explicit option is given, the latest versions of browsers now default to Lax.
            // ASP.NET Core will set the default to Lax as well.
            //o.Cookie.SameSite = SameSiteMode.Lax;

            // If we want to force a new authentication session after the cookie has expired.
            //o.SlidingExpiration = false;

            // We can set the lifetime here. This does take the sliding expiration into account.
            // A new cookie will be issued if sliding expiration is enabled,
            // and the cookie lifetime is less than half of its original age value.
            //o.Cookie.MaxAge = TimeSpan.FromDays(14);

            // This will not destroy the cookie on the browser side.
            // It will just invalidate the data inside (the authentication ticket).
            // This means the cookie will arrive at the server when the request is made.
            // As soon as it does, it is rejected.
            // The advantage of this is that we know who the user is and can add logic to handle the expired cookie (see below).
            // Typically, ExpireTimeSpan is enough to set the duration of the cookie, and it’s recommended over MaxAge for better flexibility.
            // We use the default that IdentityServer sets for their main application cookie (if not using ASP.NET Identity).
            o.ExpireTimeSpan = TimeSpan.FromHours(10);

            // This event fires every time a request comes in with an identity cookie.
            // NOTE: ASP.NET Identity already handles invalidation based on the security stamp claim.
            o.Events.OnValidatePrincipal = async context =>
            {
                bool shouldRenew = context.ShouldRenew;
                HttpContext httpContext = context.HttpContext;
                CookieAuthenticationOptions cookieOptions = context.Options;
                ClaimsPrincipal? userPrincipal = context.Principal;
                HttpRequest request = context.Request;
                AuthenticationProperties properties = context.Properties;
                HttpResponse response = context.Response;
                AuthenticationScheme scheme = context.Scheme;

                if (userPrincipal == null)
                {
                    context.RejectPrincipal();
                    return;
                }

                string? userId = userPrincipal.GetUserIdFromClaims();

                if (userId == null)
                {
                    context.RejectPrincipal();
                    return;
                }

                string? lastChanged = userPrincipal.FindFirstValue("LastUpdated");
                string? aspNetIdentitySecurityStamp = userPrincipal.FindFirstValue(AspNetIdentityClaimTypes.DefaultSecurityStampClaimType);

                UserManager<ApplicationUser> userManager = httpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();

                ApplicationUser? user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    context.RejectPrincipal();
                    return;
                }

                string userSecurityStamp = await userManager.GetSecurityStampAsync(user);

                // We can take claim data and do some validation here.
                // For example, if the user data in the db doesn't match the data in the cookie.
                // A last modified timestamp could be used, or a concurrency stamp, security stamp, etc.
                // If invalid, we can manually reject.
                // NOTE: ASP.NET Identity already does this by default, so this is just an example.
                if (aspNetIdentitySecurityStamp != userSecurityStamp)
                {
                    context.RejectPrincipal();
                    await httpContext.SignOutAsync(scheme.Name, properties);
                    return;
                }

                if (shouldRenew)
                {
                    // Renewing the cookie if it is still valid but close to expiring can improve user experience without compromising security.
                    await httpContext.SignInAsync(scheme.Name, userPrincipal, properties);
                    return;
                }
            };
        });

        // This applies additional configuration to all instances of CookieAuthenticationOptions after they have been initially configured.
        // This method is more general and can be used to override or supplement cookie authentication options set elsewhere.
        // ASP.NET Identity configures their token handler to use the "Account/AccessDenied" route internally.
        // If we want to explicitly set this, we have to do it this way.
        // These are applied to all configured cookie handlers, so this will fire multiple times.
        services.PostConfigureAll<CookieAuthenticationOptions>(o => Configure(o, httpsOptions, environment));

        return services;
    }

    private static void Configure(CookieAuthenticationOptions o, HttpsOptions httpsOptions, IHostEnvironment environment)
    {
        string? cookieName = o.Cookie.Name;

        o.LoginPath = PathConstants.LoginPath;
        o.LogoutPath = PathConstants.LogoutPath;

        o.AccessDeniedPath = PathConstants.AccessDeniedPath;

        // Complete customization can be added here
        //o.Events.OnRedirectToAccessDenied = context =>
        //{
        //    return Task.CompletedTask;
        //};

        o.ReturnUrlParameter = ParameterNames.ReturnUrl;

        // The next two options are critical for preventing cookie-based attacks
        // like Cross Site Scripting (XSS) and session hijacking.

        // Ensure the cookie cannot be accessed by JavaScript.
        o.Cookie.HttpOnly = true;

        if (!httpsOptions.UseHttpsRedirection)
        {
            o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            if (environment.IsProduction())
            {
                // This could be due to TLS termination via a reverse proxy.
                // We just want to make sure this was intended.
                Logger.LogWarning(
                    "The application has not been configured to use HTTPs redirection. " +
                    "The cookie secure policy has been set to SameAsRequest."
                );
            }
        }
        else
        {
            // Ensure it's only sent over HTTPS.
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        }

        // Using SameSiteMode.Lax is generally acceptable for authentication cookies.
        // We would consider using SameSiteMode.Strict but applications rely on cross-site requests for login.
        // We do not however, want any set to SameSiteMode.None or SameSiteMode.Unspecified.
        if (o.Cookie.SameSite is not (SameSiteMode.Lax or SameSiteMode.Strict))
        {
            o.Cookie.SameSite = SameSiteMode.Lax;
        }

        // In environments where GDPR or similar privacy laws apply,
        // marking the cookie as IsEssential helps ensure that authentication cookies can still function
        // even if the user opts out of non-essential cookies.
        o.Cookie.IsEssential = true;

        // Sliding expiration is typically enabled in cases where we'd want to extend the user's session as long as they are active.
        // This can be useful for keeping users signed in during longer sessions.
        // However, security is a priority for us, so we disable it to force re-authentication after a set period.
        o.SlidingExpiration = false;
    }
}