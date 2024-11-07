using IdentityServer.Pages;
using IdentityServer.Pages.Account;
using System.Globalization;
using static IdentityServer.Security.Authentication.Schemes.AuthenticationSchemeConstants;

namespace IdentityServer.Security.Authentication.Schemes.UserInterface;

public static class ExternalProviderService
{
    public static string GetPage(string provider)
    {
        return provider == Windows ? AccountPageConstants.WindowsLogin : PageConstants.ExternalLoginChallenge;
    }

    public static string GetProviderButtonClass(string provider)
    {
        return provider.ToLower(CultureInfo.InvariantCulture) switch
        {
            LowerCased.Google => "btn-google",
            LowerCased.Microsoft => "btn-microsoft",
            LowerCased.Spotify => "btn-spotify",
            LowerCased.Windows => "btn-windows",
            _ => "btn-generic-provider"
        };
    }

    public static string GetProviderIconClass(string provider)
    {
        return provider.ToLower(CultureInfo.InvariantCulture) switch
        {
            LowerCased.Google => "fa-brands fa-google",
            LowerCased.Microsoft => "fa-brands fa-microsoft fa-lg",
            LowerCased.Spotify => "fa-brands fa-spotify fa-lg",
            LowerCased.Windows => "fa-brands fa-windows fa-lg",
            _ => "no-icon"
        };
    }
}
