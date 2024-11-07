using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Errors.Model.Typed;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.IdentityResults;

public static class IdentityResultService
{
    public static List<ValidationError> HandleErrors(IdentityResult identityResult, ILogger logger)
    {
        foreach (IdentityError identityError in identityResult.Errors)
        {
            logger.LogWarning("{ErrorDescription}", identityError.Description);
        }

        return identityResult.Errors
            .Select(x => Error.Validation(x.Code, x.Description))
            .ToList();
    }
}