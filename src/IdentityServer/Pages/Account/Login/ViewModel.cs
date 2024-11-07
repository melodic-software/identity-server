using IdentityServer.Security.Authentication.Model;

namespace IdentityServer.Pages.Account.Login;

public class ViewModel
{
    public bool AllowRememberLogin { get; set; } = true;
    public bool EnableLocalLogin { get; set; } = true;
    public string? LoginCallbackUri { get; set; }

    public string LoginButtonValue { get; set; }
    public string CancelButtonValue { get; set; }

    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = [];
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders
        .Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

    public bool IsExternalLoginOnly => !EnableLocalLogin && ExternalProviders.Count() == 1;

    public string? ExternalLoginScheme =>
        IsExternalLoginOnly ? ExternalProviders.SingleOrDefault()?.AuthenticationScheme : null;
}
