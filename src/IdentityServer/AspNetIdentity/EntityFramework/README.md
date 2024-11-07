# Entity Framework Migration Guide

## Version History
- v1: Initial setup for AspNetIdentity migrations

## AspNetIdentity Migrations (v1)

### Add Migration

```bash
# Add Users migration
dotnet ef migrations add InitialMigration -c AspNetIdentityDbContext -s ../IdentityServer/IdentityServer.csproj -o AspNetIdentity/EntityFramework/Migrations
```

### Script Migration

```bash
# Script AspNetIdentity migration
dotnet ef migrations script -c AspNetIdentityDbContext -s ../IdentityServer/IdentityServer.csproj -o AspNetIdentity/EntityFramework/Migrations/SQL/AspNetIdentityDb.sql
```

## Notes
- Always run these commands from the root directory of this project.
- Ensure you have the latest Entity Framework Core tools installed.
- Review the generated migration files before applying them to your database.
- Migrations may need to be applied when the AspNetIdentity packages are updated.
- When adding new migrations, increment the version number (e.g., V2, V3).

```
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```