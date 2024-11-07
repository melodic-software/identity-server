﻿using Authsignal;
using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.Constants;

namespace IdentityServer.Security.Mfa.AuthSignal;

internal sealed class AuthsignalServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        string baseUrl = configuration.GetValue<string>(ConfigurationKeys.AuthsignalBaseUrl) ?? string.Empty;
        string secret = configuration.GetValue<string>(ConfigurationKeys.AuthsignalSecret) ?? string.Empty;
        services.AddSingleton<IAuthsignalClient>(_ => new AuthsignalClient(secret, baseUrl));

        services.AddScoped(provider =>
        {
            IConfiguration cfg = provider.GetRequiredService<IConfiguration>();
            IHostEnvironment env = provider.GetRequiredService<IHostEnvironment>();
            IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            IAuthsignalClient authsignalClient = provider.GetRequiredService<IAuthsignalClient>();

            return new AuthsignalTrackingService(cfg, env, httpContextAccessor, authsignalClient);
        });
    }
}