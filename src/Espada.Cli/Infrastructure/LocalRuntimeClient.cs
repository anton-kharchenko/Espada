using Espada.Cli.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Espada.Cli.Infrastructure
{
    internal sealed class LocalRuntimeClient
    {
        private static readonly Uri DaemonEndpoint = new("http://127.0.0.1:7431");
        private readonly LocalRuntimePaths _paths = new();

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(2) };
                using HttpResponseMessage response = await client.GetAsync(new Uri(DaemonEndpoint, "/health"),
                    cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (await IsHealthyAsync(cancellationToken))
            {
                return;
            }

            string executable = ResolveDaemonExecutable();
            using Process process = Process.Start(new ProcessStartInfo(executable)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = AppContext.BaseDirectory
            }) ?? throw new InvalidOperationException("Espada.Daemon could not be started.");

            DateTimeOffset deadline = DateTimeOffset.UtcNow.AddMinutes(2);
            while (DateTimeOffset.UtcNow < deadline)
            {
                if (await IsHealthyAsync(cancellationToken))
                {
                    return;
                }

                if (process.HasExited)
                {
                    throw new InvalidOperationException($"Espada.Daemon exited with code {process.ExitCode}.");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            }

            throw new TimeoutException("Espada.Daemon did not become healthy before the startup timeout.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };
            using HttpResponseMessage response = await client.PostAsync(new Uri(DaemonEndpoint, "/runtime/stop"),
                null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public LocalRuntimeState? ReadState()
        {
            return File.Exists(_paths.StateFile)
                ? JsonSerializer.Deserialize<LocalRuntimeState>(File.ReadAllText(_paths.StateFile), CliJson.Options)
                : null;
        }

        public string ReadApiKey()
        {
            return File.Exists(_paths.ApiKeyFile)
                ? File.ReadAllText(_paths.ApiKeyFile).Trim()
                : throw new InvalidOperationException("The local API key is unavailable. Start Espada first.");
        }

        private static string ResolveDaemonExecutable()
        {
            string? configuredPath = Environment.GetEnvironmentVariable("ESPADA_DAEMON_PATH");
            if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
            {
                return Path.GetFullPath(configuredPath);
            }

            string fileName = OperatingSystem.IsWindows() ? "Espada.Daemon.exe" : "Espada.Daemon";
            string adjacent = Path.Join(AppContext.BaseDirectory, fileName);
            return File.Exists(adjacent)
                ? adjacent
                : throw new FileNotFoundException(
                    "Espada.Daemon was not found next to the CLI. Set ESPADA_DAEMON_PATH for development.", adjacent);
        }
    }
}
