using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;

public class GetUserByIdQueryLogic : IQueryLogic<GetUserByIdQuery, Result<User>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<User>> ExecuteAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(query.UserId);

        if (user == null)
        {
            return Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithId(query.UserId));
        }

        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

        string firstName = userClaims.GetFirstName();
        string lastName = userClaims.GetLastName();

        return new User(user.Id, user.UserName, user.Email, user.EmailConfirmed, user.PhoneNumber, firstName, lastName);
    }
}
