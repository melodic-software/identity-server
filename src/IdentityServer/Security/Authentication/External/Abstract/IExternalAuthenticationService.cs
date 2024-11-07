using Duende.IdentityServer.Models;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Security.Authentication.Model;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Security.Authentication.External.Abstract;

// TODO: Ideally we should remove the dependency on ASP.NET Identity here.

public interface IExternalAuthenticationService
{
    Task<AuthorizationRequest?> CompleteExternalLogin(HttpContext httpContext, ExternalLoginInfo externalLoginInfo,
        AuthenticateResult authenticateResult, ApplicationUser user);
}
