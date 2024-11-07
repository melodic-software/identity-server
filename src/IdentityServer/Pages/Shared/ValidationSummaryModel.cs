using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServer.Pages.Shared;

public class ValidationSummaryModel
{
    public ValidationSummary ValidationSummary { get; set; } = ValidationSummary.All;
    public string CssClass { get; set; } = "alert alert-danger";
    public string HeaderMessage { get; set; } = "Error:";
    public bool ShowHeaderMessage { get; set; } = true;
}
