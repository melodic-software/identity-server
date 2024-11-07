using Enterprise.Patterns.ResultPattern.Errors.Model;

namespace IdentityServer.Modules.AccessManagement.UseCases.UserRoleAssignments.Shared;

public static class RoleAssignmentErrors
{
    public static Error UserNotFound =>
        Error.NotFound(RoleAssignmentErrorCodes.UserNotFound, RoleAssignmentErrorMessages.UserNotFound);
}