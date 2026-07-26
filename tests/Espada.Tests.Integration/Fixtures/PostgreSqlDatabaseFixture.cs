using Espada.Db.Database;
using Espada.Infrastructure;
using Espada.Infrastructure.Database;
using Espada.Tests.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Espada.Tests.Integration.Fixtures;

public sealed class PostgreSqlDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("espada_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using SetupDbContext dbContext = CreateSetupDbContext();
        await dbContext.Database.MigrateAsync();

        string[] pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
        if (pendingMigrations.Length > 0)
        {
            throw new InvalidOperationException($"Pending migrations remain after fixture initialization: {string.Join(", ", pendingMigrations)}");
        }
    }

    public SetupDbContext CreateSetupDbContext()
    {
        DbContextOptionsBuilder<SetupDbContext> options = new();
        options.UseNpgsql(ConnectionString, npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(SetupDbContext).Assembly.FullName);
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "Espada");
        });

        return new SetupDbContext(options.Options);
    }

    public EspadaDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<EspadaDbContext> options = new();
        options.UseNpgsql(ConnectionString);
        return new EspadaDbContext(options.Options);
    }

    public ServiceProvider CreateServiceProvider()
    {
        ServiceCollection services = new();
        services.AddInfrastructure(ConnectionString);
        return services.BuildServiceProvider();
    }

    public async Task ResetDatabaseAsync()
    {
        await using EspadaDbContext dbContext = CreateDbContext();
        await PostgreSqlDatabaseCleaner.ResetAsync(dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
