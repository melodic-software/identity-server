namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInLocalResult
{
    public string? MfaRedirectUri { get; }

    public SignInLocalResult(string? mfaRedirectUri)
    {
        MfaRedirectUri = mfaRedirectUri;
    }
}
