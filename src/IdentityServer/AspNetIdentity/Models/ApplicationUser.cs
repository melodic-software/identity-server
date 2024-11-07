using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.Models;

public sealed class ApplicationUser : IdentityUser
{
    // Add custom properties here.
    // The [PersonalData] and [ProtectedPersonalData] attributes can be used.

    [PersonalData]
    public DateTime DateCreated { get; set; }

    [PersonalData]
    public DateTime? DateEmailConfirmed { get; set; }

    [PersonalData]
    public DateTime? DateLastLoggedIn { get; set; }

    [PersonalData]
    public DateTime? DatePhoneNumberConfirmed { get; set; }
}
