namespace IdentityServer.Pages.Grants;

public class ViewModel
{
    public IEnumerable<GrantViewModel> Grants { get; set; } = Enumerable.Empty<GrantViewModel>();
}
