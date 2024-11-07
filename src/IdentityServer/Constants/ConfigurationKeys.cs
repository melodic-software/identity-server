namespace IdentityServer.Constants;

public static class ConfigurationKeys
{
    public const string AdminUserId = "AdminUser:Id";
    public const string AdminUserEmail = "AdminUser:Email";
    public const string AdminUserFirstName = "AdminUser:FirstName";
    public const string AdminUserLastName = "AdminUser:LastName";
    public const string Argon2PasswordHasher = "Argon2PasswordHasher";
    public const string AuthsignalBaseUrl = "Authsignal:BaseUrl";
    public const string AuthsignalEnabled = "Authsignal:EnableAuthsignal";
    public const string AuthsignalSecret = "Authsignal:Secret";
    public const string AuthsignalTenantId = "Authsignal:TenantId";
    public const string AzureKeyVaultEnvironmentVaultRootUri = "Azure:KeyVault:EnvironmentVaultRootUri";
    public const string AzureKeyVaultSharedVaultRootUri = "Azure:KeyVault:SharedVaultRootUri";
    public const string BCryptPasswordHasher = "BCryptPasswordHasher";
    public const string CompanyDisplayName = "CompanyDisplayName";
    public const string DataProtectionKeysAzureBlobStorageUri = "DataProtection:Keys:AzureBlobStorageUri";
    public const string DataProtectionKeysAzureKeyVaultKeyIdentifier = "DataProtection:Keys:AzureKeyVaultKeyIdentifier";
    public const string FipsComplianceRequired = "FIPSComplianceRequired";
    public const string FromEmailAddress = "FromEmailAddress";
    public const string GoogleClientId = "Authentication:Google:ClientId";
    public const string GoogleClientSecret = "Authentication:Google:ClientSecret";
    public const string Microsoft365Settings = "Microsoft365Settings";
    public const string MicrosoftClientId = "Authentication:Microsoft:ClientId";
    public const string MicrosoftClientSecret = "Authentication:Microsoft:ClientSecret";
    public const string MigrateDatabase = "MigrateDatabase";
    public const string PasswordHasherIterationCount = "PasswordHasher:IterationCount";
    public const string PreProdEmailRecipient = "PreProdEmailRecipient";
    public const string SeedData = "SeedData";
    public const string ShowErrors = "ShowErrors";
    public const string SpotifyClientId = "Authentication:Spotify:ClientId";
    public const string SpotifyClientSecret = "Authentication:Spotify:ClientSecret";
    public const string SupportEmailAddress = "SupportEmailAddress";
}
