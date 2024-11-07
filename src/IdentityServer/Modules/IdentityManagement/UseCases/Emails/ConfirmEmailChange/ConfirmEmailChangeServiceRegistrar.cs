using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Events.Handlers.Registration;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

internal sealed class ConfirmEmailChangeServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler<ConfirmEmailChangeCommand, Result, ConfirmEmailChangeCommandHandler>();
        services.RegisterEventHandler<EmailChangeConfirmedDomainEvent, EmailChangeConfirmedAuthsignalEnrollmentEventHandler>();
        services.RegisterEventHandler<EmailChangeConfirmedDomainEvent, EmailChangeConfirmedSignInRefreshEventHandler>();
    }
}