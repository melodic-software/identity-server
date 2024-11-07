using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityServer.AspNetIdentity.Dependencies.Extensions;

public static class EntityFrameworkStoreConfigExtensions
{
    public static IdentityBuilder AddEntityFrameworkStores(this IdentityBuilder builder, IServiceCollection services)
    {
        builder.AddEntityFrameworkStores<AspNetIdentityDbContext>();

        // Apparently, interfaces like IUserEmailStore<T> are not registered with the call to AddEntityFrameworkStores().
        // We ensure they are registered here so they can be referenced in application code.
        services
            .TryAddScoped<IUserEmailStore<ApplicationUser>,
                UserStore<ApplicationUser, ApplicationRole, AspNetIdentityDbContext, string>>();

        return builder;
    }
}