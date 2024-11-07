namespace IdentityServer.AspNetIdentity.Email.Senders.Microsoft365;

public class Microsoft365Settings
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string GraphUserEmailAddress { get; set; }
}
