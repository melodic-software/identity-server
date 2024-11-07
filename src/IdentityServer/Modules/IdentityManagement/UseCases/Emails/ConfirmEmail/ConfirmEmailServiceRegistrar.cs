using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Events.Handlers.Registration;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmail;

internal sealed class ConfirmEmailServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler<ConfirmEmailCommand, Result, ConfirmEmailCommandHandler>();
        services.RegisterEventHandler<EmailConfirmedDomainEvent, EmailConfirmedAuthsignalEnrollmentEventHandler>();
    }
}