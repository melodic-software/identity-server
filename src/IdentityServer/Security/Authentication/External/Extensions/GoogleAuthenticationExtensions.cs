using IdentityServer.Constants;
using IdentityServer.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace IdentityServer.Security.Authentication.External.Extensions;

public static class GoogleAuthenticationExtensions
{
    public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        return builder.AddGoogle(GoogleDefaults.AuthenticationScheme, o =>
        {
            // https://console.cloud.google.com/apis/credentials/oauthclient/1067371637968-8rvfbund5qhmbeti6ost2tkqsld5appg.apps.googleusercontent.com?project=melodic-software-420821
            o.SignInScheme = CookieSchemes.ExternalCookieAuthenticationScheme;

            o.ClientId = configuration[ConfigurationKeys.GoogleClientId] ?? string.Empty;
            o.ClientSecret = configuration[ConfigurationKeys.GoogleClientSecret] ?? string.Empty;

            o.AccessDeniedPath = PathConstants.AccessDeniedPath;
            o.ReturnUrlParameter = ParameterNames.ReturnUrl;

            o.SaveTokens = true;

            o.Events.OnRedirectToAuthorizationEndpoint = context =>
            {
                // This forces an account to be selected.
                // Without this it uses whatever account the user is logged into by default (if only logged into a single account).
                // https://developers.google.com/identity/openid-connect/openid-connect#re-consent
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                return Task.CompletedTask;
            };
        });
    }
}