using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.AspNetIdentity.Email;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.SendPasswordResetEmail;

public class SendPasswordResetEmailCommandHandler : CommandHandler<SendPasswordResetEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EmailService _emailService;

    public SendPasswordResetEmailCommandHandler(
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

    public override async Task<Result> HandleAsync(SendPasswordResetEmailCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return UserErrors.NotFoundWithId(command.UserId);
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HTTP context cannot be null.");
        }

        string passwordResetToken = await _emailService.SendPasswordResetEmailAsync(_urlHelper, httpContext, user);

        var domainEvent = new PasswordResetEmailSentDomainEvent(user.Id);

        await RaiseEventAsync(domainEvent);

        return Result.Success();
    }
}