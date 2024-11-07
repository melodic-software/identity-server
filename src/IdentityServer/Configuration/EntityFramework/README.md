# Entity Framework Migration Guide

## Version History
- v1: Initial setup for Configuration and Grant migrations

## Configuration Migrations (v1)

### Add Migrations

```bash
# Add Configuration migration
dotnet ef migrations add Configuration -c ConfigurationDbContext --output-dir Configuration/EntityFramework/Migrations/ConfigurationDb

# Add Grants migration
dotnet ef migrations add Grants -c PersistedGrantDbContext --output-dir Configuration/EntityFramework/Migrations/PersistedGrantDb
```

### Script Migrations

```bash
# Script PersistedGrant migration
dotnet ef migrations script -c PersistedGrantDbContext -o Configuration/EntityFramework/Migrations/SQL/PersistedGrantDb.sql

# Script Configuration migration
dotnet ef migrations script -c ConfigurationDbContext -o Configuration/EntityFramework/Migrations/SQL/ConfigurationDb.sql
```

## Notes
- Always run these commands from the root directory of your project.
- Ensure you have the latest Entity Framework Core tools installed.
- Review the generated migration files before applying them to your database.
- Migrations may need to be applied when the IdentityServer EF packages are updated.
- When adding new migrations, increment the version number (e.g., V2, V3).