namespace IdentityServer.Pages;

public static class PathConstants
{
    // NOTE: If route casing conventions change, this will have to be updated.
    // This is mostly used for paths in cookie options, and OIDC.
    public const string AccessDeniedPath = "/access-denied";
    public const string LoginPath = "/account/login";
    public const string LogoutPath = "/account/logout";
    public const string RegisterPath = "/account/register";
    public const string RootRelativePath = "~/";
}
