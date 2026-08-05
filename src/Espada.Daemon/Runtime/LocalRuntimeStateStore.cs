using System.Text.Json;

namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeStateStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        private readonly string _stateFile;

        public LocalRuntimeStateStore(LocalRuntimePaths paths)
        {
            ArgumentNullException.ThrowIfNull(paths);
            _stateFile = paths.StateFile;
        }

        public void Write(LocalRuntimeState state)
        {
            ArgumentNullException.ThrowIfNull(state);
            string temporaryFile = $"{_stateFile}.{Guid.NewGuid():N}.tmp";
            File.WriteAllText(temporaryFile, JsonSerializer.Serialize(state, JsonOptions));
            File.Move(temporaryFile, _stateFile, true);
        }

        public LocalRuntimeState? Read()
        {
            return File.Exists(_stateFile)
                ? JsonSerializer.Deserialize<LocalRuntimeState>(File.ReadAllText(_stateFile), JsonOptions)
                : null;
        }

        public void Delete()
        {
            if (File.Exists(_stateFile))
            {
                File.Delete(_stateFile);
            }
        }
    }
}