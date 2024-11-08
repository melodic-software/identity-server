using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.EntityFramework.Contexts.Operations.Strategical;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

public sealed class ConfirmEmailChangeCommandHandler : CommandHandler<ConfirmEmailChangeCommand, Result>
{
    private readonly AspNetIdentityDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmEmailChangeCommandHandler(
        AspNetIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public override async Task<Result> HandleAsync(ConfirmEmailChangeCommand command, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExecuteWithStrategyAsync(async () => await ConfirmEmailChangeAsync(command), cancellationToken);
    }

    private async Task<Result> ConfirmEmailChangeAsync(ConfirmEmailChangeCommand command)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(command.UserId);
        }

        string? previousEmail = user.Email;

        if (!string.IsNullOrWhiteSpace(previousEmail) &&
            previousEmail.Equals(command.Email, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        string code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Code));

        IdentityResult changeEmailResult = await _userManager.ChangeEmailAsync(user, command.Email, code);

        if (!changeEmailResult.Succeeded)
        {
            var validationErrors = changeEmailResult.Errors
                .Select(x => Error.Validation(x.Code, x.Description))
                .ToList();

            return Result.Failure(validationErrors);
        }

        // In our UI email and username are one and the same,
        // so when we update the email we need to update the username.
        IdentityResult setUserNameResult = await _userManager.SetUserNameAsync(user, command.Email);

        if (!setUserNameResult.Succeeded)
        {
            var validationErrors = changeEmailResult.Errors
                .Select(x => Error.Validation(x.Code, x.Description))
                .ToList();

            return Result.Failure(validationErrors);
        }

        var emailChangeConfirmedDomainEvent = new EmailChangeConfirmedDomainEvent(user.Id, previousEmail, command.Email);

        await RaiseEventAsync(emailChangeConfirmedDomainEvent);

        return Result.Success();
    }
}