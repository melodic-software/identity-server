using Enterprise.ApplicationServices.Core.Queries.Model.Generic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByExternalLogin;

public class GetUserByExternalLoginQuery : IQuery<Result<User>>
{
    public string ExternalProviderName { get; }
    public string ExternalUserId { get; }

    public GetUserByExternalLoginQuery(string externalProviderName, string externalUserId)
    {
        ExternalProviderName = externalProviderName;
        ExternalUserId = externalUserId;
    }
}