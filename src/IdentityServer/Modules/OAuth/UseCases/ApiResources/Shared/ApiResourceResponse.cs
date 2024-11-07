namespace IdentityServer.Modules.OAuth.UseCases.ApiResources.Shared;

public class ApiResourceResponse
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string? Description { get; set; }
    public bool Enabled { get; set; }
    public string AllowedAccessTokenSigningAlgorithms { get; set; }
    public bool ShowInDiscoveryDocument { get; set; }
    public bool RequireResourceIndicator { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public DateTime? DateLastAccessed { get; set; }
}