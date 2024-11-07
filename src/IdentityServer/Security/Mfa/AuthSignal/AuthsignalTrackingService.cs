using Authsignal;
using IdentityServer.Constants;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace IdentityServer.Security.Mfa.AuthSignal;

public class AuthsignalTrackingService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthsignalClient _authsignalClient;

    public AuthsignalTrackingService(
        IConfiguration configuration,
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        IAuthsignalClient authsignalClient)
    {
        _configuration = configuration;
        _environment = environment;
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
        dynamic? customData,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionName);

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new Exception("HTTP context is required for Authsignal MFA");
        }

        httpContext.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent);

        if (!_environment.IsProduction())
        {
            // We don't want these auth challenges going out to users in pre-production environments.
            string? preProdEmailRecipient = _configuration.GetValue<string>(ConfigurationKeys.PreProdEmailRecipient);

            if (string.IsNullOrWhiteSpace(preProdEmailRecipient))
            {
                throw new InvalidOperationException("The pre-production email recipient has not been configured.");
            }

            email = preProdEmailRecipient;
        }

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