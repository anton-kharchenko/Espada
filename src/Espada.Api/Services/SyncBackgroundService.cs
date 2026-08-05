using Espada.Infrastructure.Sync;
using Espada.Infrastructure.Sync.Contracts;
using Espada.Infrastructure.Sync.Options;
using Microsoft.Extensions.Options;

namespace Espada.Api.Services
{
    internal sealed class SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncClientOptions> options,
        SyncChangeSignal signal,
        ILogger<SyncBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int failures = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan delay = TimeSpan.FromSeconds(options.Value.PollIntervalSeconds);
                if (options.Value.IsConfigured())
                {
                    try
                    {
                        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                        await scope.ServiceProvider.GetRequiredService<ISyncClientService>()
                            .RunAsync(stoppingToken);
                        failures = 0;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        failures = 0;
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        failures = Math.Min(failures + 1, 6);
                        delay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, failures) * 5));
                        logger.LogWarning(exception,
                            "The background sync cycle failed; the local state remains pending.");
                    }
                }

                await signal.WaitAsync(delay, stoppingToken);
            }
        }
    }
}