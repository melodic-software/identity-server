namespace IdentityServer.Logging;

public static class Log
{
    private static readonly Action<ILogger, string?, Exception?> InvalidIdDelegate = LoggerMessage.Define<string?>(
        LogLevel.Error,
        EventIds.InvalidId,
        "Invalid id {Id}");

    public static void InvalidId(this ILogger logger, string? id)
    {
        InvalidIdDelegate(logger, id, null);
    }

    private static readonly Action<ILogger, string?, Exception?> InvalidBackchannelLoginIdDelegate = LoggerMessage.Define<string?>(
    LogLevel.Warning,
    EventIds.InvalidBackchannelLoginId,
    "Invalid backchannel login id {Id}");

    public static void InvalidBackchannelLoginId(this ILogger logger, string? id)
    {
        InvalidBackchannelLoginIdDelegate(logger, id, null);
    }

    private static readonly Action<ILogger, IEnumerable<string>, Exception?> ExternalClaimsDelegate = LoggerMessage.Define<IEnumerable<string>>(
        LogLevel.Debug,
        EventIds.ExternalClaims,
        "External claims: {Claims}");

    public static void ExternalClaims(this ILogger logger, IEnumerable<string> claims)
    {
        ExternalClaimsDelegate(logger, claims, null);
    }

    private static readonly Action<ILogger, string, Exception?> NoMatchingBackchannelLoginRequestDelegate = LoggerMessage.Define<string>(
        LogLevel.Error,
        EventIds.NoMatchingBackchannelLoginRequest,
        "No backchannel login request matching id: {Id}");

    public static void NoMatchingBackchannelLoginRequest(this ILogger logger, string id)
    {
        NoMatchingBackchannelLoginRequestDelegate(logger, id, null);
    }

    private static readonly Action<ILogger, string, Exception?> NoConsentMatchingRequestDelegate = LoggerMessage.Define<string>(
        LogLevel.Error,
        EventIds.NoConsentMatchingRequest,
        "No consent request matching request: {ReturnUrl}");

    public static void NoConsentMatchingRequest(this ILogger logger, string returnUrl)
    {
        NoConsentMatchingRequestDelegate(logger, returnUrl, null);
    }
}

internal static class EventIds
{
    private const int UiEventsStart = 10000;

    //////////////////////////////
    // Consent
    //////////////////////////////
    private const int ConsentEventsStart = UiEventsStart + 1000;
    public const int InvalidId = ConsentEventsStart + 0;
    public const int NoConsentMatchingRequest = ConsentEventsStart + 1;

    //////////////////////////////
    // External Login
    //////////////////////////////
    private const int ExternalLoginEventsStart = UiEventsStart + 2000;
    public const int ExternalClaims = ExternalLoginEventsStart + 0;

    //////////////////////////////
    // CIBA
    //////////////////////////////
    private const int CibaEventsStart = UiEventsStart + 3000;
    public const int InvalidBackchannelLoginId = CibaEventsStart + 0;
    public const int NoMatchingBackchannelLoginRequest = CibaEventsStart + 1;
}
