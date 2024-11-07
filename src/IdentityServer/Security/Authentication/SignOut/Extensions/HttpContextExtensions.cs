using IdentityServer.Constants;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Security.Authentication.SignOut.Extensions;

public static class HttpContextExtensions
{
    public static Task SignOutExternalAsync(this HttpContext httpContext) =>
        httpContext.SignOutAsync(CookieSchemes.ExternalCookieAuthenticationScheme);
}