using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmail;

public class ConfirmEmailCommandHandler : CommandHandler<ConfirmEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmEmailCommandHandler> logger,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<Result> HandleAsync(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(command.UserId);
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return Error.Validation(
                "User.EmailNotProvided",
                "An email address must be associated with the user account."
            );
        }

        if (user.EmailConfirmed)
        {
            _logger.LogInformation("User email address has already been confirmed.");
            return Result.Success();
        }

        // TODO: Can we just use the raw token format? See where it was generated.
        // We may be able to just use the token as it was generated.
        string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Errors.Any())
        {
            var errors = result.Errors
                .Select(x => Error.Validation(x.Code, x.Description))
                .ToList();

            return Result.Failure(errors);
        }

        _logger.LogInformation("Email address has been confirmed for user with ID: {UserId}.", user.Id);

        user.DateEmailConfirmed = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var emailConfirmedDomainEvent = new EmailConfirmedDomainEvent(user.Id, user.Email);

        await RaiseEventAsync(emailConfirmedDomainEvent);

        return Result.Success();
    }
}