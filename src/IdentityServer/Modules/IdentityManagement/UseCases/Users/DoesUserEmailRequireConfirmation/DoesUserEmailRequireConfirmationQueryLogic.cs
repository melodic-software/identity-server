using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserEmailRequireConfirmation;

public class DoesUserEmailRequireConfirmationQueryLogic : IQueryLogic<DoesUserEmailRequireConfirmationQuery, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DoesUserEmailRequireConfirmationQueryLogic(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> ExecuteAsync(DoesUserEmailRequireConfirmationQuery query,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(query.UserId);

        if (user == null || user.EmailConfirmed)
        {
            return false;
        }

        return _userManager.Options.SignIn.RequireConfirmedAccount && _userManager.SupportsUserEmail;
    }
}