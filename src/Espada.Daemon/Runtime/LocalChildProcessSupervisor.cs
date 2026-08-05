using System.Net;

namespace Espada.Daemon.Runtime
{
    public sealed class LocalChildProcessSupervisor : IAsyncDisposable
    {
        private readonly LocalRuntimeOptions _options;
        private readonly LocalRuntimePaths _paths;
        private readonly ProcessRunner _processRunner;
        private readonly Dictionary<string, ManagedChildProcess> _children = new(StringComparer.Ordinal);

        public LocalChildProcessSupervisor(LocalRuntimeOptions options, LocalRuntimePaths paths,
            ProcessRunner processRunner)
        {
            _options = options;
            _paths = paths;
            _processRunner = processRunner;
        }

        public IReadOnlyDictionary<string, int> ProcessIds => _children.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Id,
            StringComparer.Ordinal);

        public async Task StartAsync(string connectionString, string apiKey, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
            ValidatePorts();
            if (File.Exists(_paths.ShutdownFile))
            {
                File.Delete(_paths.ShutdownFile);
            }

            IReadOnlyDictionary<string, string?> commonEnvironment = new Dictionary<string, string?>
            {
                ["ESPADA_CONNECTION_STRING"] = connectionString,
                ["ESPADA_DATA_ROOT"] = _paths.Root,
                ["BlobStorage__FileSystem__RootPath"] = _paths.BlobRoot,
                ["Espada__LocalRuntime__ShutdownFile"] = _paths.ShutdownFile,
                ["DOTNET_ENVIRONMENT"] = "Production",
                ["Authentication__ApiKey__Value"] = apiKey
            };

            RuntimeExecutable database = RuntimeExecutable.Resolve("Espada.Db", _options.DbExecutable);
            ProcessResult migration = await _processRunner.RunAsync(
                database.CreateCommand(["migrate"], commonEnvironment), cancellationToken);
            await WriteMigrationLogAsync(migration, connectionString, apiKey);
            if (!migration.Succeeded)
            {
                throw new InvalidOperationException("Espada database migration failed. See migrations.log.");
            }

            StartChild("api", RuntimeExecutable.Resolve("Espada.Api", _options.ApiExecutable), [],
                With(commonEnvironment, "ASPNETCORE_URLS", $"http://127.0.0.1:{_options.ApiPort}"));
            StartChild("mcp", RuntimeExecutable.Resolve("Espada.Mcp", _options.McpExecutable), [],
                With(commonEnvironment, "ASPNETCORE_URLS", $"http://127.0.0.1:{_options.McpPort}"));
            StartChild("worker", RuntimeExecutable.Resolve("Espada.Worker", _options.WorkerExecutable), [],
                commonEnvironment);

            await WaitForReadyAsync("api", _options.ApiPort, cancellationToken);
            await WaitForReadyAsync("mcp", _options.McpPort, cancellationToken);
            if (_children["worker"].HasExited)
            {
                throw new InvalidOperationException("Espada.Worker exited during startup.");
            }
        }

        public async Task<string> WaitForUnexpectedExitAsync(CancellationToken cancellationToken)
        {
            Dictionary<Task, string> waits = _children.ToDictionary(
                entry => entry.Value.WaitForExitAsync(cancellationToken),
                entry => entry.Key);
            Task completed = await Task.WhenAny(waits.Keys);
            await completed;
            return waits[completed];
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            File.WriteAllText(_paths.ShutdownFile, DateTimeOffset.UtcNow.ToString("O"));
            TimeSpan timeout = TimeSpan.FromSeconds(_options.ShutdownTimeoutSeconds);
            await Task.WhenAll(_children.Values.Select(child => child.StopAsync(timeout, cancellationToken)));
            if (File.Exists(_paths.ShutdownFile))
            {
                File.Delete(_paths.ShutdownFile);
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (ManagedChildProcess child in _children.Values)
            {
                await child.DisposeAsync();
            }

            _children.Clear();
        }

        private void StartChild(string name, RuntimeExecutable executable, IReadOnlyList<string> arguments,
            IReadOnlyDictionary<string, string?> environment)
        {
            _children.Add(name, _processRunner.StartLongRunning(
                executable.CreateCommand(arguments, environment), _paths.LogDirectory, name));
        }

        private async Task WaitForReadyAsync(string name, int port, CancellationToken cancellationToken)
        {
            using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(2) };
            DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(_options.StartupTimeoutSeconds);
            Uri endpoint = new($"http://127.0.0.1:{port}/health/ready");
            while (DateTimeOffset.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_children[name].HasExited)
                {
                    throw new InvalidOperationException($"Espada.{name} exited during startup.");
                }

                try
                {
                    using HttpResponseMessage response = await client.GetAsync(endpoint, cancellationToken);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch (HttpRequestException)
                {
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            }

            throw new TimeoutException($"Espada.{name} did not become ready before the startup timeout.");
        }

        private void ValidatePorts()
        {
            int[] ports = [_options.ApiPort, _options.McpPort, _options.PostgresPort];
            if (ports.Distinct().Count() != ports.Length)
            {
                throw new InvalidOperationException("API, MCP, and PostgreSQL ports must be distinct.");
            }

            LoopbackPort.EnsureAvailable(_options.ApiPort);
            LoopbackPort.EnsureAvailable(_options.McpPort);
        }

        private async Task WriteMigrationLogAsync(ProcessResult migration, string connectionString, string apiKey)
        {
            await using StreamWriter writer = RotatingLogFile.Open(_paths.LogDirectory, "migrations");
            await writer.WriteLineAsync($"{DateTimeOffset.UtcNow:O} exit={migration.ExitCode}");
            if (!string.IsNullOrWhiteSpace(migration.StandardOutput))
            {
                await writer.WriteLineAsync(Redact(migration.StandardOutput, connectionString, apiKey));
            }

            if (!string.IsNullOrWhiteSpace(migration.StandardError))
            {
                await writer.WriteLineAsync(Redact(migration.StandardError, connectionString, apiKey));
            }
        }

        private static string Redact(string value, string connectionString, string apiKey)
        {
            return value.Replace(connectionString, "[redacted connection string]", StringComparison.Ordinal)
                .Replace(apiKey, "[redacted API key]", StringComparison.Ordinal);
        }

        private static IReadOnlyDictionary<string, string?> With(
            IReadOnlyDictionary<string, string?> source, string key, string value)
        {
            Dictionary<string, string?> result = new(source, StringComparer.Ordinal)
            {
                [key] = value
            };
            return result;
        }
    }
}
