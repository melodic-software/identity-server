using Duende.IdentityServer;
using IdentityServer.Constants;
using IdentityServer.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer.Security.Authentication.External.Extensions;

public static class DemoIdentityServerAuthenticationExtensions
{
    public static AuthenticationBuilder AddDemoIdentityServer(this AuthenticationBuilder builder, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return builder;
        }

        // See ConfigurationDataSeedService.cs (IdentityProviders)

        // This is a demo instance of Identity Server.
        // We don't want this in production, but can use it for testing in pre-production if needed.
        return builder.AddOpenIdConnect("demoidsrv", "Demo IdentityServer", o =>
        {
            o.SignInScheme = CookieSchemes.ExternalCookieAuthenticationScheme;
            o.SignOutScheme = IdentityServerConstants.SignoutScheme;

            o.Authority = "https://demo.duendesoftware.com";
            o.ClientId = "interactive.confidential";
            o.ClientSecret = "secret";
            o.ResponseType = "code";

            o.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };

            o.SaveTokens = true;

            o.AccessDeniedPath = PathConstants.AccessDeniedPath;
            o.ReturnUrlParameter = ParameterNames.ReturnUrl;

            //o.Events = new OpenIdConnectEvents
            //{
            //    OnRemoteFailure = context =>
            //    {
            //        context.HandleResponse();
            //        context.Response.Redirect($"{PageConstants.Error}?error=" + context.Failure?.Message);
            //        return Task.CompletedTask;
            //    }
            //};
        });
    }
}