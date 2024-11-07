using Enterprise.Applications.AspNetCore.Security.PII;
using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;

public class GetUserByEmailQueryLogic : IQueryLogic<GetUserByEmailQuery, Result<User>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByEmailQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<User>> ExecuteAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Email))
        {
            return UserErrors.EmailNotProvided;
        }

        ApplicationUser? user = await _userManager.FindByEmailAsync(query.Email);

        if (user == null)
        {
            string maskedEmail = EmailMasker.MaskEmail(query.Email);
            return Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithEmail(maskedEmail));
        }

        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

        string firstName = userClaims.GetFirstName();
        string lastName = userClaims.GetLastName();

        var result = new User(user.Id, user.Email, user.EmailConfirmed, firstName, lastName);

        return result;
    }
}