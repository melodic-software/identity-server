using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendEmailChangeConfirmationEmail;

internal sealed class SendEmailChangeConfirmationEmailServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler<SendEmailChangeConfirmationEmailCommand, Result<string>, SendEmailChangeConfirmationEmailCommandHandler>();
    }
}