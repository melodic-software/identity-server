namespace IdentityServer.Security.Authentication.Model;

public record ExternalLoginInfo
{
    public string ExternalProvider { get; }
    public string ExternalUserId { get; }
    public string ReturnUrl { get; }

    public ExternalLoginInfo(string externalProvider, string externalUserId, string returnUrl)
    {
        ExternalProvider = externalProvider;
        ExternalUserId = externalUserId;
        ReturnUrl = returnUrl;
    }
}
