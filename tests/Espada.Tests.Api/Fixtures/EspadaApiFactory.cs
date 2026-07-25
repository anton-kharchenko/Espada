using Espada.Tests.Api.TestData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Espada.Tests.Api.Fixtures;

public sealed class EspadaApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionStringVariable = "ConnectionStrings__espada";

    private readonly string? _originalConnectionString;

    public EspadaApiFactory()
    {
        _originalConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        Environment.SetEnvironmentVariable(ConnectionStringVariable, TestConnectionStrings.Espada);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ApiKey:HeaderName"] = TestValues.ApiKeyHeader,
                ["Authentication:ApiKey:Value"] = TestValues.ApiKey
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            if (disposing)
            {
                Environment.SetEnvironmentVariable(ConnectionStringVariable, _originalConnectionString);
            }
        }
    }

    public HttpClient CreateHttpsClient(bool authenticated = true)
    {
        HttpClient client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        if (authenticated)
        {
            client.DefaultRequestHeaders.Add(TestValues.ApiKeyHeader, TestValues.ApiKey);
        }

        return client;
    }
}