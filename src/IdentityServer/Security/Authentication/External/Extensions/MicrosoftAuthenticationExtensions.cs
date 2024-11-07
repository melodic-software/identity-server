using IdentityServer.Constants;
using IdentityServer.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace IdentityServer.Security.Authentication.External.Extensions;

public static class MicrosoftAuthenticationExtensions
{
    public static AuthenticationBuilder AddMicrosoft(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        // TODO: Restrict this to just Microsoft accounts?
        builder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, o =>
        {
            // https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/1591fe98-67a1-4588-bb69-13aa04defb61/isMSAApp~/false
            o.SignInScheme = CookieSchemes.ExternalCookieAuthenticationScheme;

            o.ClientId = configuration[ConfigurationKeys.MicrosoftClientId] ?? string.Empty;
            o.ClientSecret = configuration[ConfigurationKeys.MicrosoftClientSecret] ?? string.Empty;

            o.AccessDeniedPath = PathConstants.AccessDeniedPath;
            o.ReturnUrlParameter = ParameterNames.ReturnUrl;

            //o.CallbackPath = new PathString("/signin-microsoft");

            o.Scope.Add("openid");
            o.Scope.Add("profile");
            o.Scope.Add("email");
            o.Scope.Add("offline_access");

            o.SaveTokens = true;

            o.Events.OnRedirectToAuthorizationEndpoint = context =>
            {
                // This forces an account to be selected.
                // Without this it uses whatever account the user is logged into by default (if only logged into a single account).
                // https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return Task.CompletedTask;
            };
        });

        // Use this for employee login?
        //builder.AddOpenIdConnect("ME-ID", "Employee Login", options =>
        //{
        //    options.SignInScheme = SharedConstants.ExternalCookieAuthenticationScheme;
        //    options.ClientId = "";
        //    options.ClientSecret = "";
        //});

        return builder;
    }
}