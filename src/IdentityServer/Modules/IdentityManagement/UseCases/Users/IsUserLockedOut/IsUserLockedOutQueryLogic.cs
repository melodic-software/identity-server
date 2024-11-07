using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserLockedOut;

public class IsUserLockedOutQueryLogic : IQueryLogic<IsUserLockedOutQuery, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IsUserLockedOutQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> ExecuteAsync(IsUserLockedOutQuery query, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(query.UserId);

        if (user == null)
        {
            return false;
        }

        bool userIsLockedOut = await _userManager.IsLockedOutAsync(user);

        return userIsLockedOut;
    }
}