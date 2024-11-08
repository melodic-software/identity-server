using Enterprise.ApplicationServices.Core.Queries.Model.Generic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.UserClaims;

public class GetUserClaimsQuery : IQuery<Result<ICollection<Claim>>>
{
    public string UserId { get; }

    public GetUserClaimsQuery(string userId)
    {
        UserId = userId;
    }
}