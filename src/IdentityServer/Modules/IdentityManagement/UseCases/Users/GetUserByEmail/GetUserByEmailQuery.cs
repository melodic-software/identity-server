using Enterprise.ApplicationServices.Core.Queries.Model.Generic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;

public class GetUserByEmailQuery : IQuery<Result<User>>
{
    public string Email { get; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}