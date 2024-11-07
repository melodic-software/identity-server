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

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;

public class SendConfirmationEmailCommandHandler : CommandHandler<SendConfirmationEmailCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EmailService _emailService;

    public SendConfirmationEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUrlHelper urlHelper,
        IHttpContextAccessor httpContextAccessor,
        EmailService emailService,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _userManager = userManager;
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
    }

    public override async Task<Result<string>> HandleAsync(SendConfirmationEmailCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(command.UserId);
        }

        if (user.EmailConfirmed || !_userManager.SupportsUserEmail)
        {
            return Result.Success(string.Empty);
        }

        if (user.Email == null)
        {
            return Error.Validation("User.NoEmailAddress", "User does not have an email address. Cannot send confirmation email.");
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HTTP context cannot be null.");
        }

        string encodedToken = await _emailService.SendConfirmationEmailAsync(_urlHelper, httpContext, user, user.Email, command.ReturnUrl);

        var confirmationEmailSentDomainEvent = new ConfirmationEmailSentDomainEvent(user.Id, user.Email);
        
        await RaiseEventAsync(confirmationEmailSentDomainEvent);

        return Result.Success(encodedToken);
    }
}