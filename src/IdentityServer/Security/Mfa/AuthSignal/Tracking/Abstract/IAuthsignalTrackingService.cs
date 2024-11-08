using Authsignal;

namespace IdentityServer.Security.Mfa.AuthSignal.Tracking.Abstract;

public interface IAuthsignalTrackingService
{
    public Task<TrackResponse> GetTrackResponseAsync(
        string actionName,
        string? redirectUrl,
        string userId,
        string? username,
        string? email,
        string? phoneNumber,
        string? deviceId,
        Dictionary<string, string>? customData,
        CancellationToken cancellationToken);
}