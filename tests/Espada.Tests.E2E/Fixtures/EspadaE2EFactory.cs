using Espada.Db.Constants;
using Espada.Db.Database;
using Espada.Infrastructure.Database;
using Espada.Tests.E2E.TestData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Espada.Tests.E2E.Fixtures;

public sealed class EspadaE2EFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(E2ETestValues.PostgreSqlImage)
        .WithDatabase(E2ETestValues.PostgreSqlDatabase)
        .WithUsername(E2ETestValues.PostgreSqlUsername)
        .WithPassword(E2ETestValues.PostgreSqlPassword)
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        _factory = new TestingWebApplicationFactory(_container.GetConnectionString());

        await using SetupDbContext dbContext = CreateSetupDbContext();
        await dbContext.Database.MigrateAsync();

        string[] pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
        if (pendingMigrations.Length > 0)
        {
            throw new InvalidOperationException($"Pending migrations remain after E2E fixture initialization: {string.Join(", ", pendingMigrations)}");
        }
    }

    private IServiceProvider Services => (_factory ?? throw new InvalidOperationException("E2E factory is not initialized.")).Services;

    public HttpClient CreateClient(bool authenticated = true)
    {
        WebApplicationFactory<Program> factory = _factory ?? throw new InvalidOperationException("E2E factory is not initialized.");

        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        if (authenticated)
        {
            client.DefaultRequestHeaders.Add(E2ETestValues.ApiKeyHeader, E2ETestValues.ApiKey);
        }

        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        await using SetupDbContext dbContext = CreateSetupDbContext();
        await PostgreSqlDatabaseCleaner.ResetAsync(dbContext);
    }

    private SetupDbContext CreateSetupDbContext()
    {
        DbContextOptions<SetupDbContext> options =
            PostgreSqlDbContextOptions.Create<SetupDbContext>(
                _container.GetConnectionString(),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(
                        typeof(SetupDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        DbConstants.SchemaName);
                });

        return new SetupDbContext(options);
    }
    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}