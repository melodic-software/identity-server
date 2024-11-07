using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Events.Facade.Abstract;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;

internal sealed class ResetPasswordServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler(provider =>
        {
            UserManager<ApplicationUser> userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            ILogger<ResetPasswordCommandHandler> logger = provider.GetRequiredService<ILogger<ResetPasswordCommandHandler>>();
            IEventRaisingFacade eventService = provider.GetRequiredService<IEventRaisingFacade>();

            return new ResetPasswordCommandHandler(userManager, logger, eventService);
        });
    }
}