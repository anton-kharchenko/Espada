using Espada.Db.Database;
using Espada.Infrastructure;
using Espada.Application.Extensions;
using Espada.Infrastructure.Database;
using Espada.Tests.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Espada.Tests.Integration.Fixtures;

public sealed class PostgreSqlDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("pgvector/pgvector:0.8.2-pg17")
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
        return new SetupDbContext(
            PostgreSqlDbContextOptions.Create<SetupDbContext>(
                ConnectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(SetupDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "Espada");
                }));
    }

    public EspadaDbContext CreateDbContext()
    {
        return new EspadaDbContext(
            PostgreSqlDbContextOptions.Create<EspadaDbContext>(ConnectionString));
    }

    public ServiceProvider CreateServiceProvider(
        IConfiguration? configuration = null,
        Action<IServiceCollection>? configureServices = null)
    {
        ServiceCollection services = new();
        services.ConfigureApplicationLayer();
        services.AddInfrastructure(ConnectionString, configuration);
        configureServices?.Invoke(services);
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