using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Admin.ApiScopes;

public class ApiScopeSummaryModel
{
    public int ApiScopeId { get; set; }

    [Display(Name = "API Scope Name")]
    [Required]
    public string Name { get; set; }

    [Display(Name = "Display Name")]
    public string? DisplayName { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Is Enabled")]
    public bool IsEnabled { get; set; }
}
