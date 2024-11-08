using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetLoggedInUser;

public class GetLoggedInUserQueryLogic : IQueryLogic<GetLoggedInUserQuery, Result<User>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetLoggedInUserQueryLogic(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<Result<User>> ExecuteAsync(GetLoggedInUserQuery query, CancellationToken cancellationToken = default)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HTTP context cannot be null.");
        }

        ClaimsPrincipal claimsPrincipal = httpContext.User;

        ApplicationUser? user = await _userManager.GetUserAsync(claimsPrincipal);

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

        string firstName = userClaims.GetFirstName();
        string lastName = userClaims.GetLastName();

        return new User(user.Id, user.UserName, user.Email, user.EmailConfirmed, user.PhoneNumber, firstName, lastName);
    }
}