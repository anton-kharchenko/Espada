using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Espada.Tests.Daemon.Endpoints
{
    public sealed class HealthEndpointTests
    {
        [Fact]
        public async Task Health_ReturnsSuccess_WhenSupervisorIsDisabled()
        {
            await using WebApplicationFactory<Espada.Daemon.Program> application =
                new WebApplicationFactory<Espada.Daemon.Program>()
                    .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Espada:LocalRuntime:Enabled"] = "false"
                        });
                    }));
            using HttpClient client = application.CreateClient();

            using HttpResponseMessage response = await client.GetAsync("/health");

            response.EnsureSuccessStatusCode();
        }
    }
}
