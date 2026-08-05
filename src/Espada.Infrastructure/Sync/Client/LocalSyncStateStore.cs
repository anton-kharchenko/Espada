using System.Security.Cryptography;
using System.Text.Json;

namespace Espada.Infrastructure.Sync.Client
{
    internal sealed class LocalSyncStateStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly string _path;

        public LocalSyncStateStore()
        {
            string root = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                          ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                              "Espada");
            _path = Path.Join(root, "sync-state.json");
        }

        public async Task<LocalSyncState> ReadAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_path))
            {
                return new LocalSyncState("0");
            }

            await using FileStream stream = File.OpenRead(_path);
            return await JsonSerializer.DeserializeAsync<LocalSyncState>(stream, SerializerOptions,
                       cancellationToken)
                   ?? new LocalSyncState("0");
        }

        public async Task WriteAsync(LocalSyncState state, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            string temporary = _path + ".tmp-" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8));
            await using (FileStream stream = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, state, SerializerOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temporary, _path, true);
        }
    }
}