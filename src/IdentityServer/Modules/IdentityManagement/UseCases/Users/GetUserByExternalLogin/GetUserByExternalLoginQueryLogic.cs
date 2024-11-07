using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByExternalLogin;

public class GetUserByExternalLoginQueryLogic : IQueryLogic<GetUserByExternalLoginQuery, Result<User>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByExternalLoginQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<User>> ExecuteAsync(GetUserByExternalLoginQuery query, CancellationToken cancellationToken = default)
    {
        // Find a local account associated with the external provider name and key.
        // This will be null if they initially created a local account without using an external login
        // and have not yet separately linked this provider after logging in.
        ApplicationUser? user = await _userManager
            .FindByLoginAsync(query.ExternalProviderName, query.ExternalUserId);

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

        string firstName = userClaims.GetFirstName();
        string lastName = userClaims.GetLastName();

        var result = new User(user.Id, user.Email, user.EmailConfirmed, firstName, lastName);

        return result;
    }
}