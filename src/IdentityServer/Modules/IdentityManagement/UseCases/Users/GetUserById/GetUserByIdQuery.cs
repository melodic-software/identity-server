using Enterprise.ApplicationServices.Core.Queries.Model.Generic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;

public class GetUserByIdQuery : IQuery<Result<User>>
{
    public string UserId { get; }

    public GetUserByIdQuery(string userId)
    {
        UserId = userId;
    }
}