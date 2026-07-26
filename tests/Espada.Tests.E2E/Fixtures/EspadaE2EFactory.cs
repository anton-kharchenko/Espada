using Espada.Infrastructure.Database;
using Espada.Tests.E2E.TestData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Espada.Tests.E2E.Fixtures;

public sealed class EspadaE2EFactory : IAsyncLifetime
{
    private const string ConnectionStringVariable = "ConnectionStrings__espada";
    private const string ApiKeyVariable = "Authentication__ApiKey__Value";
    private const string ApiKeyHeaderVariable = "Authentication__ApiKey__HeaderName";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("espada_e2e_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private string? _originalConnectionString;
    private string? _originalApiKey;
    private string? _originalApiKeyHeader;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        _originalConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        _originalApiKey = Environment.GetEnvironmentVariable(ApiKeyVariable);
        _originalApiKeyHeader = Environment.GetEnvironmentVariable(ApiKeyHeaderVariable);

        Environment.SetEnvironmentVariable(ConnectionStringVariable, _container.GetConnectionString());
        Environment.SetEnvironmentVariable(ApiKeyVariable, E2ETestValues.ApiKey);
        Environment.SetEnvironmentVariable(ApiKeyHeaderVariable, E2ETestValues.ApiKeyHeader);

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });

        using IServiceScope scope = Services.CreateScope();
        EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
        await dbContext.Database.MigrateAsync();
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

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _container.DisposeAsync();

        Environment.SetEnvironmentVariable(ConnectionStringVariable, _originalConnectionString);
        Environment.SetEnvironmentVariable(ApiKeyVariable, _originalApiKey);
        Environment.SetEnvironmentVariable(ApiKeyHeaderVariable, _originalApiKeyHeader);
    }
}