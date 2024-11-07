using Enterprise.Configuration.Docker;
using Enterprise.DesignPatterns.Decorator.Instances.Abstract;
using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.AspNetIdentity.Email.Senders;
using IdentityServer.AspNetIdentity.Email.Senders.Decorators;
using IdentityServer.AspNetIdentity.Email.Senders.Microsoft365;
using IdentityServer.AspNetIdentity.Email.Templates.Services;
using IdentityServer.Constants;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace IdentityServer.AspNetIdentity.Email.Dependencies;

internal sealed class EmailServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<Microsoft365Settings>(configuration.GetSection(ConfigurationKeys.Microsoft365Settings));

        services.AddScoped<EmailTemplateService>();
        services.AddScoped<EmailService>();

        if (environment.IsDevelopment() && CurrentApplication.IsRunningInDocker())
        {
            // In local development environments where we are using Docker, we expect to use a container for local SMTP.
            // We don't need to decorate here and reroute emails since they should all be routed locally.
            // We're using a generic email service, which uses IConfiguration for values.
            // It is expected that the host and port be aligned with whatever dev container we're using.
            // Typically, this is smpt4dev or Papercut.

            services.AddScoped<IEmailSender>(provider =>
            {
                IConfiguration cfg = provider.GetRequiredService<IConfiguration>();
                return new EmailSender(cfg);
            });
        }
        else
        {
            services.AddScoped(provider =>
            {
                IConfiguration cfg = provider.GetRequiredService<IConfiguration>();
                IOptions<Microsoft365Settings> options = provider.GetRequiredService<IOptions<Microsoft365Settings>>();
                ILogger<Microsoft365EmailSender> microsoftLogger = provider.GetRequiredService<ILogger<Microsoft365EmailSender>>();

                IEmailSender emailSender = new Microsoft365EmailSender(options, microsoftLogger);

                if (environment.IsProduction())
                {
                    return emailSender;
                }
                
                IGetDecoratedInstance decoratorService = provider.GetRequiredService<IGetDecoratedInstance>();
                ILogger<PreProdEmailRecipientDecorator> nonProdLogger = provider.GetRequiredService<ILogger<PreProdEmailRecipientDecorator>>();

                // We want to decorate with a service that ensures real recipient email addresses are not used.
                emailSender = new PreProdEmailRecipientDecorator(cfg,emailSender, decoratorService, nonProdLogger);

                return emailSender;
            });
        }
    }
}
