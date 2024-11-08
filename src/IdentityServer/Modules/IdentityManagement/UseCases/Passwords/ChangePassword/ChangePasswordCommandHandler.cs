using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ChangePassword;

public class ChangePasswordCommandHandler : CommandHandler<ChangePasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ChangePasswordCommandHandler> logger,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<Result> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(command.UserId);
        }

        if (command.CurrentPassword == command.NewPassword)
        {
            return Error.Validation("PasswordChange.SamePassword", "Your new password must be different from the current password.");
        }

        IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        var errors = new List<IError>();

        if (!changePasswordResult.Succeeded)
        {
            errors.AddRange(changePasswordResult.Errors
                .Select(error => Error.Validation(error.Code, error.Description)));

            return Result.Failure(errors);
        }

        _logger.LogInformation("User changed their password successfully.");

        var passwordChangedDomainEvent = new PasswordChangedDomainEvent(user.Id);

        await RaiseEventAsync(passwordChangedDomainEvent);

        return Result.Success();
    }
}