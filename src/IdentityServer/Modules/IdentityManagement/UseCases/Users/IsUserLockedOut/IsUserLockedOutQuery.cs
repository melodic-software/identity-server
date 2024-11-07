using Enterprise.ApplicationServices.Core.Queries.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserLockedOut;

public class IsUserLockedOutQuery : IQuery<bool>
{
    public string UserId { get; }

    public IsUserLockedOutQuery(string userId)
    {
        UserId = userId;
    }
}