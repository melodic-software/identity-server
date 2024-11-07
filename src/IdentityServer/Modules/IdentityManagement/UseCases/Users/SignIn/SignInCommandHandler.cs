using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Security.Authentication.SignIn;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInCommandHandler : CommandHandler<SignInCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;

    public SignInCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IEventService events,
        IEventRaisingFacade eventRaisingFacade) : base(eventRaisingFacade)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _events = events;
    }

    public override async Task<Result<bool>> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);

        if (user == null)
        {
            return Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithId(command.UserId));
        }

        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(command.ReturnUrl);

        await _signInManager.SignInAsync(user, isPersistent: command.RememberLogin);
        await SignInSuccessService.HandleSuccessfulSignInAsync(user, _userManager, authorizationRequest, _events);

        return true;
    }
}