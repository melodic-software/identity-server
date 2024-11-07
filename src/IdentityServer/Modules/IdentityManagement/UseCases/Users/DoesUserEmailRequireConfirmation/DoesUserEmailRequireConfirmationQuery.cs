using Enterprise.ApplicationServices.Core.Queries.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserEmailRequireConfirmation;

public class DoesUserEmailRequireConfirmationQuery : IQuery<bool>
{
    public string UserId { get; }

    public DoesUserEmailRequireConfirmationQuery(string userId)
    {
        UserId = userId;
    }
}