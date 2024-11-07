using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignOut;

public class SignOutCommandHandler : CommandHandler<SignOutCommand, Result>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEventService _events;
    private readonly ILogger<SignOutCommandHandler> _logger;

    public SignOutCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        SignInManager<ApplicationUser> signInManager,
        IEventService events,
        ILogger<SignOutCommandHandler> logger,
        IEventRaisingFacade eventRaisingService) : base(eventRaisingService)
    {
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
        _events = events;
        _logger = logger;
    }

    public override async Task<Result> HandleAsync(SignOutCommand command, CancellationToken cancellationToken = default)
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
        bool userIsNotSignedIn = user is null || !_signInManager.IsSignedIn(user);

        if (userIsNotSignedIn)
        {
            _logger.LogInformation("No user is authenticated. Sign out is not necessary.");
            return Result.Success();
        }
        
        string? subjectId = user.GetSubjectId();
        string? userDisplayName = user.GetDisplayName();
        string? idp = user!.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

        // Delete the local authentication cookie.
        // https://docs.duendesoftware.com/identityserver/v7/ui/logout/session_cleanup/
        await _signInManager.SignOutAsync();

        _logger.LogInformation("The user has been signed out.");
        
        var userLogoutSuccessEvent = new UserLogoutSuccessEvent(subjectId, userDisplayName);
        await _events.RaiseAsync(userLogoutSuccessEvent);
        Telemetry.Metrics.UserLogout(idp);

        return Result.Success();
    }
}