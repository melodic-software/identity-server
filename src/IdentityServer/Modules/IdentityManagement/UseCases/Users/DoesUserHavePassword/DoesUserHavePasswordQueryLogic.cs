using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserHavePassword;

public class DoesUserHavePasswordQueryLogic : IQueryLogic<DoesUserHavePasswordQuery, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DoesUserHavePasswordQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> ExecuteAsync(DoesUserHavePasswordQuery query, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(query.UserId);

        if (user == null)
        {
            return false;
        }

        bool userHasPassword = await _userManager.HasPasswordAsync(user);

        return userHasPassword;
    }
}