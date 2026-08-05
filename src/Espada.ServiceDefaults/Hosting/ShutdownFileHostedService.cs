using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Espada.ServiceDefaults.Hosting
{
    internal sealed class ShutdownFileHostedService(
        IConfiguration configuration,
        IHostApplicationLifetime applicationLifetime) : BackgroundService
    {
        public const string ConfigurationKey = "Espada:LocalRuntime:ShutdownFile";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string? shutdownFile = configuration[ConfigurationKey];
            if (string.IsNullOrWhiteSpace(shutdownFile))
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (File.Exists(shutdownFile))
                {
                    applicationLifetime.StopApplication();
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken);
            }
        }
    }
}
