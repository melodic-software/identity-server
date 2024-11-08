using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Email;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendEmailChangeConfirmationEmail;

public class SendEmailChangeConfirmationEmailCommandHandler : CommandHandler<SendEmailChangeConfirmationEmailCommand, Result<string>>
{
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public SendEmailChangeConfirmationEmailCommandHandler(
        IUrlHelper urlHelper,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        EmailService emailService,
        IEventRaisingFacade eventService
    ) : base(eventService)
    {
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _emailService = emailService;
    }

    public override async Task<Result<string>> HandleAsync(SendEmailChangeConfirmationEmailCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(command.UserId);
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return UserErrors.EmailNotProvided;
        }

        if (string.IsNullOrWhiteSpace(command.NewEmail))
        {
            return Error.Validation("NewEmail.MustBeProvided", "New email must be provided.");
        }

        string newEmail = command.NewEmail.Trim();

        if (user.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation("NewEmail.MustBeDifferent", "The new email address must be different from the old email address.");
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HTTP context cannot be null.");
        }

        string encodedToken = await _emailService.SendEmailChangeConfirmationEmailAsync(_urlHelper, httpContext, user, newEmail, returnUrl: null);

        return Result.Success(encodedToken);
    }
}