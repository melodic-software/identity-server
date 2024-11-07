using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace IdentityServer.Security.Claims;
public static class ExternalClaimsMapper
{
    public static void MapMicrosoftClaims(MicrosoftAccountOptions options)
    {
        options.ClaimActions.MapJsonKey("urn:microsoft:sub", "sub", "string");
        options.ClaimActions.MapJsonKey("urn:microsoft:name", "name", "string");
        options.ClaimActions.MapJsonKey("urn:microsoft:given_name", "given_name", "string");
        options.ClaimActions.MapJsonKey("urn:microsoft:family_name", "family_name", "string");
        options.ClaimActions.MapJsonKey("urn:microsoft:profile", "profile", "url");
        options.ClaimActions.MapJsonKey("urn:microsoft:email", "email", "string");
        options.ClaimActions.MapJsonKey("urn:microsoft:email_verified", "email_verified", "boolean");
        options.ClaimActions.MapJsonKey("urn:microsoft:locale", "locale", "string");
    }

    public static void MapGoogleClaims(GoogleOptions options)
    {
        options.ClaimActions.MapJsonKey("urn:google:sub", "sub", "string");
        options.ClaimActions.MapJsonKey("urn:google:name", "name", "string");
        options.ClaimActions.MapJsonKey("urn:google:given_name", "given_name", "string");
        options.ClaimActions.MapJsonKey("urn:google:family_name", "family_name", "string");
        options.ClaimActions.MapJsonKey("urn:google:profile", "profile", "url");
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
        options.ClaimActions.MapJsonKey("urn:google:email", "email", "string");
        options.ClaimActions.MapJsonKey("urn:google:email_verified", "email_verified", "boolean");
        options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
        options.ClaimActions.MapJsonKey("urn:google:hd", "hd", "string");
    }
}
