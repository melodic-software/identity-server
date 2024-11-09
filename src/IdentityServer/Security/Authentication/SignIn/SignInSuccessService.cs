using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Telemetry = IdentityServer.Observability.Diagnostics.Telemetry;

namespace IdentityServer.Security.Authentication.SignIn;

public static class SignInSuccessService
{
    public static async Task HandleSuccessfulSignInAsync(ApplicationUser user, UserManager<ApplicationUser> userManager,
        AuthorizationRequest? authorizationRequest, IEventService events)
    {
        user.DateLastLoggedIn = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        var userLoginSuccessEvent = new UserLoginSuccessEvent(
            user.UserName,
            user.Id,
            user.UserName,
            clientId: authorizationRequest?.Client.ClientId
        );

        await events.RaiseAsync(userLoginSuccessEvent);

        Telemetry.Metrics.UserLogin(authorizationRequest?.Client.ClientId,
            IdentityServerConstants.LocalIdentityProvider);
    }

    public static async Task HandleSuccessfulExternalSignInAsync(ApplicationUser user, UserManager<ApplicationUser> userManager,
        string externalProvider, string externalUserId, AuthorizationRequest? authorizationRequest, IEventService events)
    {
        user.DateLastLoggedIn = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        var userLoginSuccess = new UserLoginSuccessEvent(
            externalProvider,
            externalUserId,
            user.Id,
            user.UserName,
            true,
            authorizationRequest?.Client.ClientId
        );

        await events.RaiseAsync(userLoginSuccess);

        Telemetry.Metrics.UserLogin(authorizationRequest?.Client.ClientId, externalProvider);
    }
}
