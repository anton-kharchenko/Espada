using Microsoft.Extensions.Options;

namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeHostedService(
        IOptions<LocalRuntimeOptions> options,
        LocalRuntimeStatus status,
        IHostApplicationLifetime applicationLifetime,
        ILogger<LocalRuntimeHostedService> logger) : BackgroundService
    {
        private readonly LocalRuntimeOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                status.Set("healthy");
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
                return;
            }

            LocalRuntimePaths paths = new(_options.DataRoot);
            ProcessRunner processRunner = new();
            DockerPostgresSupervisor postgres = new(_options, processRunner);
            LocalChildProcessSupervisor children = new(_options, paths, processRunner);
            LocalRuntimeLock? runtimeLock = null;
            LocalRuntimeStateStore? stateStore = null;
            DateTimeOffset startedAtUtc = DateTimeOffset.UtcNow;
            bool postgresStarted = false;

            try
            {
                await postgres.EnsureDockerAvailableAsync(stoppingToken);
                paths.EnsureCreated();
                runtimeLock = LocalRuntimeLock.Acquire(paths);
                stateStore = new LocalRuntimeStateStore(paths);
                string password = new PostgresPasswordStore(paths).GetOrCreate();
                string apiKey = new LocalApiKeyStore(paths).GetOrCreate();
                await postgres.StartAsync(paths, stoppingToken);
                postgresStarted = true;

                string connectionString =
                    $"Host=127.0.0.1;Port={_options.PostgresPort};Database=Espada;Username=espada;" +
                    $"Password={password};Include Error Detail=false";
                await children.StartAsync(connectionString, apiKey, stoppingToken);
                status.Set("healthy");
                stateStore.Write(CreateState("healthy", startedAtUtc, children.ProcessIds));

                string exitedChild = await children.WaitForUnexpectedExitAsync(stoppingToken);
                if (!stoppingToken.IsCancellationRequested)
                {
                    status.Set("unhealthy");
                    stateStore.Write(CreateState("unhealthy", startedAtUtc, children.ProcessIds));
                    logger.LogError("Required child process {ChildName} exited unexpectedly.", exitedChild);
                    applicationLifetime.StopApplication();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                status.Set("unhealthy");
                stateStore?.Write(CreateState("unhealthy", startedAtUtc, children.ProcessIds));
                logger.LogCritical(exception, "Espada local runtime failed.");
                applicationLifetime.StopApplication();
            }
            finally
            {
                status.Set("stopping");
                try
                {
                    await children.StopAsync(CancellationToken.None);
                    if (postgresStarted)
                    {
                        await postgres.StopAsync(CancellationToken.None);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Espada local runtime shutdown failed.");
                }

                await children.DisposeAsync();
                stateStore?.Write(CreateState("stopped", startedAtUtc, new Dictionary<string, int>()));
                runtimeLock?.Dispose();
                status.Set("stopped");
            }
        }

        private LocalRuntimeState CreateState(string currentStatus, DateTimeOffset startedAtUtc,
            IReadOnlyDictionary<string, int> childProcessIds)
        {
            return new LocalRuntimeState(Environment.ProcessId, currentStatus, startedAtUtc, _options.ApiPort,
                _options.McpPort, _options.PostgresPort, _options.PostgresContainerName, childProcessIds);
        }
    }
}
