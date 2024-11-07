using AspNet.Security.OAuth.Spotify;
using IdentityServer.Constants;
using IdentityServer.Pages;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Security.Authentication.External.Extensions;

public static class SpotifyAuthenticationExtensions
{
    public static AuthenticationBuilder AddSpotify(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        return builder.AddSpotify(SpotifyAuthenticationDefaults.AuthenticationScheme, o =>
        {
            o.SignInScheme = CookieSchemes.ExternalCookieAuthenticationScheme;

            o.ClientId = configuration[ConfigurationKeys.SpotifyClientId] ?? string.Empty;
            o.ClientSecret = configuration[ConfigurationKeys.SpotifyClientSecret] ?? string.Empty;

            o.AccessDeniedPath = PathConstants.AccessDeniedPath;
            o.ReturnUrlParameter = ParameterNames.ReturnUrl;

            //o.CallbackPath = new PathString("/signin-spotify");

            o.SaveTokens = true;

            // https://developer.spotify.com/documentation/web-api/concepts/scopes
            o.Scope.Add("user-read-private");
            o.Scope.Add("user-read-email");

            o.UsePkce = true;

            o.Events.OnRedirectToAuthorizationEndpoint = context =>
            {
                // Spotify doesn't have an account select prompt like Google and Microsoft.
                // We can use this query string parameter to force consent, which is better than nothing.
                context.Response.Redirect(context.RedirectUri + "&show_dialog=true");
                return Task.CompletedTask;
            };
        });
    }
}