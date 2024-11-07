using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Admin.ApiScopes;

public class ApiScopeModel : ApiScopeSummaryModel
{
    public bool Required { get; set; }

    public bool Emphasize { get; set; }

    [Display(Name = "Show In Discovery Document")]
    public bool ShowInDiscoveryDocument { get; set; }

    [Display(Name = "Is Non Editable")]
    public bool IsNonEditable { get; set; }

    public string? UserClaims { get; set; }
}