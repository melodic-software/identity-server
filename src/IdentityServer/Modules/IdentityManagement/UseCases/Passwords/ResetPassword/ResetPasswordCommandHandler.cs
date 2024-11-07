using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.AspNetIdentity.IdentityResults;
using IdentityServer.AspNetIdentity.IdentityResults.Extensions;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;

public class ResetPasswordCommandHandler : CommandHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ResetPasswordCommandHandler> logger,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        if (string.IsNullOrWhiteSpace(command.PasswordResetToken))
        {
            return Error.Validation("PasswordReset.TokenRequired", "A password reset token is required.");
        }

        IdentityResult identityResult = await _userManager.ResetPasswordAsync(user, command.PasswordResetToken, command.NewPassword);

        if (identityResult.WasNotSuccessful())
        {
            return IdentityResultService.HandleErrors(identityResult, _logger).ToResult();
        }

        var domainEvent = new PasswordResetDomainEvent(user.Id);

        await RaiseEventAsync(domainEvent);

        return Result.Success();
    }
}