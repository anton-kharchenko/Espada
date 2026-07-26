using Espada.Infrastructure.Database;
using Espada.Db;
using Microsoft.EntityFrameworkCore;
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

        await using EspadaDbContext dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public EspadaDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<EspadaDbContext> options = new();

        options.UseNpgsql(ConnectionString, npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(EspadaDbAssembly).Assembly.FullName);
        });

        return new EspadaDbContext(options.Options);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}