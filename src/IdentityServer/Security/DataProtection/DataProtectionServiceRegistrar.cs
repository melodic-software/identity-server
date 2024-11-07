using Azure.Identity;
using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.Constants;
using Microsoft.AspNetCore.DataProtection;

namespace IdentityServer.Security.DataProtection;

internal sealed class DataProtectionServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        string? blobUri = configuration[ConfigurationKeys.DataProtectionKeysAzureBlobStorageUri];
        string? keyIdentifier = configuration[ConfigurationKeys.DataProtectionKeysAzureKeyVaultKeyIdentifier];

        if (string.IsNullOrWhiteSpace(blobUri))
        {
            throw new InvalidOperationException("Cannot configure data protection without a BLOB storage URI");
        }

        if (string.IsNullOrWhiteSpace(keyIdentifier))
        {
            throw new InvalidOperationException("Cannot configure data protection using a key from Azure Key Vault without a key identifier.");
        }

        var credential = new DefaultAzureCredential();

        services.AddDataProtection()
            .PersistKeysToAzureBlobStorage(new Uri(blobUri), credential)
            .ProtectKeysWithAzureKeyVault(new Uri(keyIdentifier), credential)
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
    }
}
