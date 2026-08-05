using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Infrastructure.Sync.Authentication
{
    internal sealed class SyncTokenStore
    {
        private const string ServiceName = "Espada.Cloud";
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly string _windowsPath;

        public SyncTokenStore()
        {
            string root = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                          ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                              "Espada");
            _windowsPath = Path.Join(root, "secrets", "cloud-token");
        }

        public async Task<SyncTokenSet?> ReadAsync(CancellationToken cancellationToken)
        {
            string? json;
            if (OperatingSystem.IsWindows())
            {
                if (!File.Exists(_windowsPath))
                {
                    return null;
                }

                byte[] protectedBytes = await File.ReadAllBytesAsync(_windowsPath, cancellationToken);
                byte[] clearBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                json = Encoding.UTF8.GetString(clearBytes);
                CryptographicOperations.ZeroMemory(clearBytes);
            }
            else if (OperatingSystem.IsMacOS())
            {
                json = await RunAsync("security",
                    ["find-generic-password", "-a", Environment.UserName, "-s", ServiceName, "-w"], null,
                    cancellationToken, allowFailure: true);
            }
            else
            {
                json = await RunAsync("secret-tool",
                    ["lookup", "service", ServiceName, "account", Environment.UserName], null,
                    cancellationToken, allowFailure: true);
            }

            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<SyncTokenSet>(json, SerializerOptions);
        }

        public async Task WriteAsync(SyncTokenSet token, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(token);
            string json = JsonSerializer.Serialize(token, SerializerOptions);
            if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_windowsPath)!);
                byte[] clearBytes = Encoding.UTF8.GetBytes(json);
                byte[] protectedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.CurrentUser);
                CryptographicOperations.ZeroMemory(clearBytes);
                string temporary = _windowsPath + ".tmp-" +
                                   Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8));
                await File.WriteAllBytesAsync(temporary, protectedBytes, cancellationToken);
                File.Move(temporary, _windowsPath, true);
                return;
            }

            if (OperatingSystem.IsMacOS())
            {
                await RunAsync("security",
                    ["add-generic-password", "-a", Environment.UserName, "-s", ServiceName, "-w", json, "-U"], null,
                    cancellationToken);
                return;
            }

            await RunAsync("secret-tool",
                ["store", "--label=Espada Cloud", "service", ServiceName, "account", Environment.UserName],
                json, cancellationToken);
        }

        private static async Task<string?> RunAsync(string fileName, IReadOnlyList<string> arguments, string? input,
            CancellationToken cancellationToken, bool allowFailure = false)
        {
            ProcessStartInfo startInfo = new(fileName)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = input is not null,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process process = Process.Start(startInfo)
                                    ?? throw new InvalidOperationException($"Unable to start {fileName}.");
            if (input is not null)
            {
                await process.StandardInput.WriteAsync(input.AsMemory(), cancellationToken);
                process.StandardInput.Close();
            }

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            string error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0 && !allowFailure)
            {
                throw new InvalidOperationException(
                    $"The operating system credential store failed with exit code {process.ExitCode}: {error.Trim()}");
            }

            return process.ExitCode == 0 ? output.Trim() : null;
        }
    }
}