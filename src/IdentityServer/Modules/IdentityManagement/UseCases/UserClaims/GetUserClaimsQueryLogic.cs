using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.UserClaims;

public class GetUserClaimsQueryLogic : IQueryLogic<GetUserClaimsQuery, Result<ICollection<Claim>>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserClaimsQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<ICollection<Claim>>> ExecuteAsync(GetUserClaimsQuery query, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(query.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(query.UserId);
        }

        IList<Claim> claims = await _userManager.GetClaimsAsync(user);

        return claims.ToList();
    }
}