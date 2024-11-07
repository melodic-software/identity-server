namespace IdentityServer.Security.Authentication.Model;

public record ExternalProvider
{
    public ExternalProvider(string authenticationScheme, string? displayName = null)
    {
        AuthenticationScheme = authenticationScheme;
        DisplayName = displayName;
    }

    public string? DisplayName { get; set; }
    public string AuthenticationScheme { get; set; }
}
