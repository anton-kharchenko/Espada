using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Espada.Tests.Api.Fixtures;

public sealed class EspadaApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            Dictionary<string, string?> values = new()
            {
                ["ConnectionStrings:espada"] = "Host=localhost;Port=5432;Database=espada_api_tests;Username=postgres;Password=postgres"
            };

            configuration.AddInMemoryCollection(values);
        });
    }

    public HttpClient CreateHttpsClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }
}