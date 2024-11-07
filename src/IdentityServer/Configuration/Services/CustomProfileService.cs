using Authsignal;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Configuration.Services;

// https://docs.duendesoftware.com/identityserver/v7/quickstarts/5_aspnetid/
// Other methods can be overriden here.
// NOTE: Do not use IClaimsTransformer as those are mostly intended for client applications.
// The profile service is IdentityServer's mechanism.

public class CustomProfileService : ProfileService<ApplicationUser>
{
    private readonly IConfiguration _configuration;
    private readonly IAuthsignalClient _authsignalClient;
    private readonly ILogger<CustomProfileService> _logger;

    public CustomProfileService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IAuthsignalClient authsignalClient,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        ILogger<CustomProfileService> logger) 
        : base(userManager, claimsFactory)
    {
        _configuration = configuration;
        _authsignalClient = authsignalClient;
        _logger = logger;
    }

    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
    {
        try
        {
            // This loads user claims from the identity store.
            ClaimsPrincipal? principal = await GetUserClaimsAsync(user);
            var claimsIdentity = (ClaimsIdentity)principal.Identity;

            string? subjectId = context.Subject.GetSubjectId();

            // Add the custom "amr" claim based on the authentication method.
            await AuthenticationMethodReferenceClaimService.AddAmrClaimAsync(claimsIdentity, user, _configuration, _authsignalClient);

            // We can add additional claims for values that may be stored externally from the ASP.NET Identity claims table.
            // These could be stored on the user record, or an external system.
            AddCustomProfileDataClaims(user, claimsIdentity);

            context.AddRequestedClaims(principal.Claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading profile data for user {UserId}.", user.Id);
            throw;
        }
    }

    private static void AddCustomProfileDataClaims(ApplicationUser user, ClaimsIdentity? id)
    {
        if (id == null)
        {
            return;
        }

        // Some extended user properties may need to be added as available claims.
        // The code below shows how a property can be translated to a claim.
        //if (!string.IsNullOrEmpty(user.FavoriteColor))
        //{
        //    id.AddClaim(new Claim("favorite_color", user.FavoriteColor));
        //}
    }
}
