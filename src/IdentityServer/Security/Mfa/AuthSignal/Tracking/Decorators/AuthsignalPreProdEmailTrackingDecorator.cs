using Authsignal;
using Enterprise.DesignPatterns.Decorator.Instances.Abstract;
using Enterprise.DesignPatterns.Decorator.Model.Generic.Base;
using IdentityServer.Security.Mfa.AuthSignal.Tracking.Abstract;

namespace IdentityServer.Security.Mfa.AuthSignal.Tracking.Decorators;

public class AuthsignalPreProdEmailTrackingDecorator : Decorator<IAuthsignalTrackingService>, IAuthsignalTrackingService
{
    private readonly string _emailAddress;

    public AuthsignalPreProdEmailTrackingDecorator(
        string emailAddress,
        IAuthsignalTrackingService decorated,
        IGetDecoratedInstance decoratorService) : base(decorated, decoratorService)
    {
        _emailAddress = emailAddress;
    }

    public async Task<TrackResponse> GetTrackResponseAsync(string actionName, string? redirectUrl, string userId, string? username, string? email,
        string? phoneNumber, string? deviceId, Dictionary<string, string>? customData, CancellationToken cancellationToken)
    {
        // We're just replacing all emails with this hard coded email address that is used in pre-production environments.
        return await Decorated.GetTrackResponseAsync(actionName, redirectUrl, userId, username, _emailAddress,
            phoneNumber, deviceId, customData, cancellationToken);
    }
}
