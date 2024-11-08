using Enterprise.Events.Handlers.Abstract.Generic.Base;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

public class EmailChangeConfirmedSignInRefreshEventHandler : EventHandlerBase<EmailChangeConfirmedDomainEvent>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<EmailChangeConfirmedSignInRefreshEventHandler> _logger;

    public EmailChangeConfirmedSignInRefreshEventHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<EmailChangeConfirmedSignInRefreshEventHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public override async Task HandleAsync(EmailChangeConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(@event.UserId);

        if (user == null)
        {
            _logger.LogWarning("User with ID \"{UserId}\" could not be located.", @event.UserId);
            return;
        }

        // TODO: Do we need to move this so it only executes when the command is being executed locally (IdentityServer).

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("Sign in refreshed for user with ID: {UserId}.", user.Id);
    }
}