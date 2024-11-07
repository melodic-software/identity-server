using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.EntityFramework.Contexts.Operations.Strategical;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.IdentityResults;
using IdentityServer.AspNetIdentity.IdentityResults.Extensions;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public class RegisterUserCommandHandler : CommandHandler<RegisterUserCommand, Result<string>>
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly AspNetIdentityDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IAuthenticationSchemeProvider schemeProvider,
        AspNetIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterUserCommandHandler> logger,
        IEventRaisingFacade eventService) : base(eventService)
    {
        _schemeProvider = schemeProvider;
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<Result<string>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExecuteWithStrategyAsync(async () => await RegisterUser(command), cancellationToken);
    }

    private async Task<Result<string>> RegisterUser(RegisterUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return UserErrors.EmailNotProvided;
        }

        ApplicationUser? user = await _userManager.FindByEmailAsync(command.Email);

        if (user != null)
        {
            // TODO: Does this follow security best practices? Should they know if the email account has already been registered?
            return UserErrors.EmailAlreadyRegistered(command.Email);
        }

        string userId = command.UserId ?? Guid.NewGuid().ToString();

        user = new ApplicationUser
        {
            Id = userId,

            // TODO: Generate unique username.
            // Start with the email, check for uniqueness
            // Add some random values if needed.
            // It might be fun to generate some music related names by default.
            UserName = command.Email, // Right now we're defaulting to email since it should be unique.

            Email = command.Email,
            EmailConfirmed = false,

            DateCreated = DateTime.UtcNow
        };

        IdentityResult identityResult = command.IsExternalLogin
            ? await _userManager.CreateAsync(user)
            : await _userManager.CreateAsync(user, command.Password!);

        if (identityResult.WasNotSuccessful())
        {
            return IdentityResultService.HandleErrors(identityResult, _logger).ToResult<string>();
        }

        if (command.IsExternalLogin)
        {
            AuthenticationScheme authenticationScheme = await _schemeProvider.GetSchemeAsync(command.ExternalProvider!) ??
                                                        throw new InvalidOperationException(ErrorMessages.AuthenticationSchemeNotFound);

            var userLoginInfo = new UserLoginInfo(authenticationScheme.Name, command.ExternalUserId!, authenticationScheme.DisplayName);

            identityResult = await _userManager.AddLoginAsync(user, userLoginInfo);

            if (identityResult.WasNotSuccessful())
            {
                return IdentityResultService.HandleErrors(identityResult, _logger).ToResult<string>();
            }
        }

        List<Claim> claims = await PopulateClaimsAsync(user, command);

        if (claims.Any())
        {
            identityResult = await _userManager.AddClaimsAsync(user, claims);
        }

        if (identityResult.WasNotSuccessful())
        {
            return IdentityResultService.HandleErrors(identityResult, _logger).ToResult<string>();
        }

        var userRegisteredDomainEvent = new UserRegisteredDomainEvent(user.Id, command.ReturnUrl);

        await RaiseEventAsync(userRegisteredDomainEvent);

        return Result<string>.Success(user.Id);
    }

    private async Task<List<Claim>> PopulateClaimsAsync(ApplicationUser user, RegisterUserCommand command)
    {
        var claims = new List<Claim>();

        claims.SetUserRegistrationClaims(command.FirstName, command.LastName);

        // Get existing user claims.
        // There shouldn't be any, but this allows us to load, add, and dedupe.
        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

        // Use a dictionary to filter and deduplicate claims.
        var existingClaimTypes = userClaims.ToDictionary(uc => uc.Type, uc => uc.Value);

        // Add only new claims.
        var filteredClaims = claims.Where(c => !existingClaimTypes.ContainsKey(c.Type)).ToList();

        return filteredClaims;
    }
}