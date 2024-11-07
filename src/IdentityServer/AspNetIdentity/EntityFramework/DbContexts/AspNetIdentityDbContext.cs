using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.AspNetIdentity.EntityFramework.DbContexts;

public class AspNetIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AspNetIdentityDbContext(DbContextOptions<AspNetIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Here we can customize the ASP.NET Identity model and override the defaults if needed.
        builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
