using Azure.Core;
using Azure.Identity;
using IdentityServer.Constants;

namespace IdentityServer.Security.KeyVaults;

// TODO: Add constants.
// TODO: Implement caching with Azure Key Vault provider(s) here, so we don't hit the vault every time.
// We'll also need real time cache invalidation - likely implemented with Azure Event Grid and RabbitMQ.
// TODO: Integrate into enterprise library, add configuration/options.

// https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-8.0

public static class AzureKeyVaultService
{
    public static void AddAzureKeyVault(this IHostApplicationBuilder builder)
    {
        // User secrets should be used in local development.
        if (builder.Environment.IsDevelopment())
        {
            return;
        }

        TokenCredential credential = GetCredential();

        // These use the KeyVaultSecretManager by default.
        // That manager translates key vault names like "Key--NestedKey" into the expected "Key:NestedKey" format.
        AddSharedVault(builder, credential);
        AddEnvironmentVault(builder, credential);
    }

    public static Uri GetVaultUriFromName(string vaultName)
    {
        return new Uri($"https://{vaultName}.vault.azure.net/");
    }

    private static DefaultAzureCredential GetCredential()
    {
        return new DefaultAzureCredential();
    }

    private static void AddSharedVault(IHostApplicationBuilder builder, TokenCredential credential)
    {
        AddVault(builder, credential, ConfigurationKeys.AzureKeyVaultSharedVaultRootUri);
    }

    private static void AddEnvironmentVault(IHostApplicationBuilder builder, TokenCredential credential)
    {
        AddVault(builder, credential, ConfigurationKeys.AzureKeyVaultEnvironmentVaultRootUri);
    }

    private static void AddVault(IHostApplicationBuilder builder, TokenCredential credential, string configKey)
    {
        string? vaultRootUri = builder.Configuration[configKey];

        if (string.IsNullOrWhiteSpace(vaultRootUri))
        {
            throw new Exception($"The value for \"{configKey}\" has not been configured.");
        }

        var vaultUri = new Uri(vaultRootUri);

        builder.Configuration.AddAzureKeyVault(vaultUri, credential);
    }
}
