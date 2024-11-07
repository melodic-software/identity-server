using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Constants;

public static class CookieSchemes
{
    public static readonly string CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;
    public static readonly string ExternalCookieAuthenticationScheme = IdentityConstants.ExternalScheme;
}
