using Authsignal;
using Enterprise.Http.Constants;
using IdentityServer.Security.Mfa.AuthSignal.Tracking.Abstract;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace IdentityServer.Security.Mfa.AuthSignal.Tracking;

public class AuthsignalTrackingService : IAuthsignalTrackingService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthsignalClient _authsignalClient;

    public AuthsignalTrackingService(
       
        IHttpContextAccessor httpContextAccessor,
        IAuthsignalClient authsignalClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _authsignalClient = authsignalClient;
    }

    public async Task<TrackResponse> GetTrackResponseAsync(
        string actionName,
        string? redirectUrl,
        string userId,
        string? username,
        string? email,
        string? phoneNumber,
        string? deviceId,
        Dictionary<string, string>? customData,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionName);

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new Exception("HTTP context is required for Authsignal MFA.");
        }

        httpContext.Request.Headers.TryGetValue(HttpHeaderNames.UserAgent, out StringValues userAgent);

        IPAddress? ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        var trackRequest = new TrackRequest(
            UserId: userId,
            Username: username,
            Email: email,
            Action: actionName,
            RedirectUrl: redirectUrl,
            PhoneNumber: phoneNumber,
            IpAddress: ipAddress?.ToString(),
            UserAgent: userAgent,
            DeviceId: deviceId,
            Custom: customData
        );

        TrackResponse trackResponse = await _authsignalClient.Track(trackRequest, cancellationToken);

        return trackResponse;
    }
}