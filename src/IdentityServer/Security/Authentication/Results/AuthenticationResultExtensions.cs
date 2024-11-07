using IdentityServer.Constants;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace IdentityServer.Security.Authentication.Results;

public static class AuthenticationResultExtensions
{
    public static async Task<AuthenticateResult> AuthenticateExternalAsync(this HttpContext httpContext)
    {
        // Read external identity from the temporary cookie.
        return await httpContext.AuthenticateAsync(CookieSchemes.ExternalCookieAuthenticationScheme);
    }

    public static ClaimsPrincipal GetPrincipal(this AuthenticateResult result)
    {
        ClaimsPrincipal externalUser = result.Principal;

        if (externalUser == null)
        {
            throw new InvalidOperationException(ErrorMessages.ExternalAuthNullPrincipal);
        }

        return externalUser;
    }

    public static string GetExternalProvider(this AuthenticateResult result)
    {
        string? externalProvider = result.Properties?.Items[ParameterNames.Scheme];

        if (string.IsNullOrWhiteSpace(externalProvider))
        {
            throw new InvalidOperationException(ErrorMessages.AuthPropertiesNullScheme);
        }

        return externalProvider;
    }

    public static Uri GetReturnUrl(this AuthenticateResult result, string? defaultValue = null)
    {
        string? returnUrl = result.Properties?.Items[ParameterNames.ReturnUrl];

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            returnUrl = defaultValue;
        }

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            throw new InvalidOperationException(ErrorMessages.InvalidReturnUrl);
        }

        return new Uri(returnUrl, UriKind.RelativeOrAbsolute);
    }
}
