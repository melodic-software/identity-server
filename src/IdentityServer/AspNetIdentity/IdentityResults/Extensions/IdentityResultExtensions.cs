using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.IdentityResults.Extensions;

public static class IdentityResultExtensions
{
    public static bool WasNotSuccessful(this IdentityResult result) => !result.Succeeded;
}
