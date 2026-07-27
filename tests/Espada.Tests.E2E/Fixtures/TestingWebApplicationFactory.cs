using Microsoft.AspNetCore.Mvc.Testing;

namespace Espada.Tests.E2E.Fixtures;

internal sealed class TestingWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .UseSetting("ConnectionStrings:Espada", connectionString)
            .ConfigureAppConfiguration(configuration => configuration.AddJsonFile(Path.Join(AppContext.BaseDirectory, "appsettings.Testing.json"), optional: false));
    }
}