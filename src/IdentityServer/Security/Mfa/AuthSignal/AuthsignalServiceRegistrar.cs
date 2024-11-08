using Authsignal;
using Enterprise.DesignPatterns.Decorator.Instances.Abstract;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.DI.Registration.Context;
using Enterprise.DI.Registration.Context.Extensions;
using Enterprise.Logging.Core.Startup;
using IdentityServer.Constants;
using IdentityServer.Security.Mfa.AuthSignal.Tracking;
using IdentityServer.Security.Mfa.AuthSignal.Tracking.Abstract;
using IdentityServer.Security.Mfa.AuthSignal.Tracking.Decorators;

namespace IdentityServer.Security.Mfa.AuthSignal;

internal sealed class AuthsignalServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        string baseUrl = configuration.GetValue<string>(ConfigurationKeys.AuthsignalBaseUrl) ?? string.Empty;
        string secret = configuration.GetValue<string>(ConfigurationKeys.AuthsignalSecret) ?? string.Empty;
        services.AddSingleton<IAuthsignalClient>(_ => new AuthsignalClient(secret, baseUrl));

        RegistrationContext<IAuthsignalTrackingService> trackingServiceContext = services
            .BeginRegistration<IAuthsignalTrackingService>()
            .AddScoped<IAuthsignalTrackingService, AuthsignalTrackingService>();

        if (!environment.IsProduction())
        {
            // We don't want these auth challenges going out to users in pre-production environments.
            string? preProdEmailRecipient = configuration.GetValue<string>(ConfigurationKeys.PreProdEmailRecipient);

            if (string.IsNullOrWhiteSpace(preProdEmailRecipient))
            {
                throw new InvalidOperationException("The pre-production email recipient has not been configured.");
            }

            StartupLogger.Instance.LogInformation(
                "Registering pre-production decorator for Authsignal tracking. " +
                "Email to be used: {PreProdEmailRecipient}",
                preProdEmailRecipient
            );

            trackingServiceContext.WithDecorator((provider, authsignalTrackingService) =>
            {
                IGetDecoratedInstance decoratorService = provider.GetRequiredService<IGetDecoratedInstance>();

                return new AuthsignalPreProdEmailTrackingDecorator(preProdEmailRecipient, authsignalTrackingService, decoratorService);
            });
        }
    }
}
