using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Events.Handlers.Registration;
using Enterprise.Events.Integration;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

internal sealed class RegisterUserServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler(provider =>
        {
            IAuthenticationSchemeProvider schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
            AspNetIdentityDbContext dbContext = provider.GetRequiredService<AspNetIdentityDbContext>();
            UserManager<ApplicationUser> userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            ILogger<RegisterUserCommandHandler> logger = provider.GetRequiredService<ILogger<RegisterUserCommandHandler>>();
            IEventRaisingFacade eventRaiser = provider.GetRequiredService<IEventRaisingFacade>();

            return new RegisterUserCommandHandler(schemeProvider, dbContext, userManager, logger, eventRaiser);
        });

        services.RegisterEventHandler(provider =>
        {
            IQueryDispatchFacade queryDispatcher = provider.GetRequiredService<IQueryDispatchFacade>();
            IIntegrationEventBus eventBus = provider.GetRequiredService<IIntegrationEventBus>();

            return new UserRegisteredEventHandler(queryDispatcher, eventBus);
        });

        services.RegisterEventHandler<UserRegisteredDomainEvent, UserRegisteredSendConfirmationEmailEventHandler>();
    }
}