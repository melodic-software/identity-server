using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.Authentication.OpenIdConnect.Constants;
using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Security.Authentication.External.Abstract;
using IdentityServer.Security.Authentication.SignIn;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ExternalLoginInfo = IdentityServer.Security.Authentication.Model.ExternalLoginInfo;

namespace IdentityServer.Security.Authentication.External;

// TODO: Ideally we should remove the dependency on ASP.NET Identity here.

public class ExternalAuthenticationService : IExternalAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;

    public ExternalAuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IEventService events)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _events = events;
    }

    public async Task<AuthorizationRequest?> CompleteExternalLogin(
        HttpContext httpContext,
        ExternalLoginInfo externalLoginInfo,
        AuthenticateResult authenticateResult,
        ApplicationUser user)
    {
        string externalProvider = externalLoginInfo.ExternalProvider;
        string externalUserId = externalLoginInfo.ExternalUserId;
        string returnUrl = externalLoginInfo.ReturnUrl;

        // This allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // This is typically used to store data needed for signout from those protocols.
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(authenticateResult, additionalLocalClaims, localSignInProps);

        // Issue authentication cookie for user.
        await _signInManager.SignInWithClaimsAsync(user, localSignInProps, additionalLocalClaims);

        // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/additional-claims?view=aspnetcore-8.0
        //await ExternalClaimsSyncService.SyncExternalClaimsAsync(user, result, _userManager, _signInManager);
        //await ExternalTokenService.StoreExternalTokensAsync(user, result, _userManager);

        // Delete temporary cookie used during external authentication.
        await httpContext.SignOutExternalAsync();

        // Check if external login is in the context of an OIDC request.
        AuthorizationRequest? context = await _interaction.GetAuthorizationContextAsync(returnUrl);

        await SignInSuccessService.HandleSuccessfulExternalSignInAsync(user, _userManager, externalProvider, externalUserId, context, _events);

        return context;
    }

    // If the external login is OIDC-based, there are certain things we need to preserve to make logout work.
    // This will be different for WS-Fed, SAML2p or other protocols.
    private static void CaptureExternalLoginContext(
        AuthenticateResult authenticateResult,
        List<Claim> localClaims,
        AuthenticationProperties localSignInProps)
    {
        ArgumentNullException.ThrowIfNull(authenticateResult.Principal);

        // Capture the idp used to log in, so the session knows where the user came from.
        string idpClaimValue = authenticateResult.Properties?.Items[ParameterNames.Scheme] ?? IdentityProviders.Unknown;
        var idpClaim = new Claim(JwtClaimTypes.IdentityProvider, idpClaimValue);
        localClaims.Add(idpClaim);

        // If the external system sent a session id claim, copy it over, so we can use it for single sign-out.
        Claim? sid = authenticateResult.Principal.Claims
            .FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);

        if (sid != null)
        {
            var sessionIdClaim = new Claim(JwtClaimTypes.SessionId, sid.Value);
            localClaims.Add(sessionIdClaim);
        }

        // If the external provider issued an id_token, we'll keep it for signout.
        string? idToken = authenticateResult.Properties?.GetTokenValue(OpenIdConnectConstants.IdToken);

        if (idToken != null)
        {
            localSignInProps.StoreTokens([new AuthenticationToken
            {
                Name = OpenIdConnectConstants.IdToken,
                Value = idToken
            }]);
        }
    }
}
