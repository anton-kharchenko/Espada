using Espada.Api.LocalSetup.Models;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Espada.Api.LocalSetup.Services
{
    internal sealed class LocalRuntimeConfigurationWriter
    {
        public void Validate(CommitLocalSetupRequest request, LocalSetupPortPreview current)
        {
            int[] ports = [request.ApiPort, request.McpPort, request.PostgresPort];
            if (ports.Distinct().Count() != ports.Length)
            {
                throw new ArgumentException("API, MCP, and PostgreSQL ports must be distinct.");
            }

            EnsureAvailableWhenChanged(request.ApiPort, current.Api);
            EnsureAvailableWhenChanged(request.McpPort, current.Mcp);
            EnsureAvailableWhenChanged(request.PostgresPort, current.PostgreSql);
        }

        public async Task WriteAsync(CommitLocalSetupRequest request, LocalSetupPortPreview current,
            CancellationToken cancellationToken)
        {
            Validate(request, current);
            string root = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada");
            Directory.CreateDirectory(root);
            string path = Path.Join(root, "runtime.json");
            string temporary = path + ".tmp";
            object configuration = new
            {
                Espada = new
                {
                    LocalRuntime = new
                    {
                        request.ApiPort,
                        request.McpPort,
                        request.PostgresPort
                    }
                }
            };
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(configuration,
                new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true }), cancellationToken);
            File.Move(temporary, path, true);
        }

        private static void EnsureAvailableWhenChanged(int selectedPort, int currentPort)
        {
            if (selectedPort == currentPort)
            {
                return;
            }

            TcpListener listener = new(IPAddress.Loopback, selectedPort);
            try
            {
                listener.Start();
            }
            catch (SocketException)
            {
                throw new ArgumentException($"Loopback port {selectedPort} is already in use.");
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
