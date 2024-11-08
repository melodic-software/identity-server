using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Email;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendEmailChangeConfirmationEmail;

public class SendEmailChangeConfirmationEmailCommandHandler : CommandHandler<SendEmailChangeConfirmationEmailCommand, Result<string>>
{
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly EmailService _emailService;

    public SendEmailChangeConfirmationEmailCommandHandler(
        IUrlHelper urlHelper,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        EmailService emailService,
        IEventRaisingFacade eventService
    ) : base(eventService)
    {
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _emailSender = emailSender;
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

        string encodedToken = await _emailService.GenerateChangeEmailTokenAsync(user, newEmail);
        Uri callbackUrl = _emailService.GenerateEmailChangeConfirmationLink(_urlHelper, httpContext, user.Id, encodedToken, newEmail);

        // TODO: Complete converting this, absorb as much into the email sender as the others.

        await _emailSender.SendEmailAsync(
            newEmail,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl.ToString())}'>clicking here</a>.");


        return Result.Success(encodedToken);
    }
}