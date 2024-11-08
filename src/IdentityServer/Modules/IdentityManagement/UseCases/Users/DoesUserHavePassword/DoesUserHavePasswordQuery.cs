using Enterprise.ApplicationServices.Core.Queries.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserHavePassword;

public sealed class DoesUserHavePasswordQuery : IQuery<bool>
{
    public string UserId { get; }

    public DoesUserHavePasswordQuery(string userId)
    {
        UserId = userId;
    }
}