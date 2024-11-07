using Duende.IdentityServer.Models;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class ExternalSignIn
{
    public bool Succeeded { get; }
    public AuthorizationRequest? AuthorizationRequest { get; }

    public ExternalSignIn(bool succeeded, AuthorizationRequest? authorizationRequest)
    {
        Succeeded = succeeded;
        AuthorizationRequest = authorizationRequest;
    }
}
