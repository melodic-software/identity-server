using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RefreshSignIn;

public class RefreshSignInCommandHandler : CommandHandler<RefreshSignInCommand, Result>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RefreshSignInCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public override async Task<Result> HandleAsync(RefreshSignInCommand command, CancellationToken cancellationToken = default)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return Result.Failure(Error.Validation("HTTP context cannot be null."));
        }

        ClaimsPrincipal claimsPrincipal = httpContext.User;

        ApplicationUser? user = await _userManager.GetUserAsync(claimsPrincipal);

        if (user == null)
        {
            return Result.Success();
        }

        await _signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }
}