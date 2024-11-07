namespace IdentityServer.Modules.OAuth.UseCases.ApiResources.CreateApiResource;

public class CreateApiResourceRequest
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
}