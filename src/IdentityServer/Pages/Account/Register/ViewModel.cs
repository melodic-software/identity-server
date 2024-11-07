using IdentityServer.Security.Authentication.Model;

namespace IdentityServer.Pages.Account.Register;

public class ViewModel
{
    public PasswordConfiguration PasswordConfig { get; set; }
    public string WelcomeMessage { get; set; }

    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = [];
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders
        .Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));
}
