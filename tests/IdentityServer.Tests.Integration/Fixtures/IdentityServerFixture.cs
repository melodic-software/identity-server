using Duende.IdentityServer.EntityFramework.DbContexts;
using Enterprise.ApplicationServices.Core.Commands.Dispatching;
using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.EntityFramework.Contexts.Operations.Strategical;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.Tests.Integration.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.Tests.Integration.Fixtures;

// https://courses.dometrain.com/courses/take/from-zero-to-hero-vertical-slice-architecture/lessons/56735133-getting-ready-for-integration-testing

public sealed class IdentityServerFixture : IDisposable
{
    private readonly IdentityServerWebApplicationFactory _factory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IServiceScope? _scope;

    private static readonly object Lock = new();
    private static bool _databasesInitialized;

    public IdentityServerFixture()
    {
        _factory = new IdentityServerWebApplicationFactory();

        // The default implementation is provided by the ASP.NET Core framework.
        _serviceScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        lock (Lock)
        {
            if (_databasesInitialized)
            {
                return;
            }

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                InitializeDbContext<AspNetIdentityDbContext>(scope);
                InitializeDbContext<ConfigurationDbContext>(scope);
                InitializeDbContext<PersistedGrantDbContext>(scope);
            }

            _databasesInitialized = true;
        }
    }

    public void InitializeDbContext<T>(IServiceScope scope) where T : DbContext
    {
        using T context = CreateDbContext<T>(scope);
        //context.Database.EnsureDeleted();
        //context.Database.EnsureCreatedAsync();
        context.Database.MigrateAsync();
    }

    public T CreateDbContext<T>(IServiceScope scope) where T : DbContext
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public async Task ExecuteTestAsync<T>(Func<T, Task> operation) where T : DbContext
    {
        using (_scope = _serviceScopeFactory.CreateScope())
        {
            T dbContext = CreateDbContext<T>(_scope);
            await dbContext.ExecuteWithStrategyAsync(async () =>
            {
                await operation(dbContext);
            }, useTransaction: true, commitTransaction: false);
        }
    }

    public async Task<T> DispatchCommandAsync<T>(ICommand<T> command)
    {
        if (_scope == null)
        {
            throw new ArgumentException($"{nameof(DispatchCommandAsync)} must be run in the context of a scope.");
        }

        IDispatchCommands commandDispatcher = _scope.ServiceProvider.GetRequiredService<IDispatchCommands>();

        return await commandDispatcher.DispatchAsync(command, CancellationToken.None);
    }

    public void Dispose()
    {
        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
        {
            // Ensure we remove our integration test specific databases.
            // We could leave these around locally IF we needed to inspect records after test execution.
            scope.ServiceProvider.GetRequiredService<AspNetIdentityDbContext>().Database.EnsureDeleted();
            scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.EnsureDeleted();
            scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.EnsureDeleted();
        }

        _factory.Dispose();
    }
}
