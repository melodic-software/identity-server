using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Base;
using Enterprise.Events.Facade.Abstract;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserSignedIn;

public class IsUserSignedInQueryHandler : QueryHandler<IsUserSignedInQuery, bool>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IsUserSignedInQueryHandler(IHttpContextAccessor httpContextAccessor,
        SignInManager<ApplicationUser> signInManager,
        IEventRaisingFacade eventRaisingFacade) : base(eventRaisingFacade)
    {
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
    }

    public override Task<bool> HandleAsync(IsUserSignedInQuery query, CancellationToken cancellationToken = default)
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            return Task.FromResult(false);
        }

        bool isSignedIn = _signInManager.IsSignedIn(user);

        return Task.FromResult(isSignedIn);
    }
}