namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimePaths
    {
        public LocalRuntimePaths(string? configuredRoot)
        {
            Root = string.IsNullOrWhiteSpace(configuredRoot)
                ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada")
                : Path.GetFullPath(configuredRoot);
            LockFile = Path.Join(Root, "daemon.lock");
            PidFile = Path.Join(Root, "daemon.pid");
            StateFile = Path.Join(Root, "runtime-state.json");
            RuntimeConfigurationFile = Path.Join(Root, "runtime.json");
            PasswordFile = Path.Join(Root, "secrets", "postgres-password");
            ApiKeyFile = Path.Join(Root, "secrets", "local-api-key");
            ShutdownFile = Path.Join(Root, "shutdown.request");
            BlobRoot = Path.Join(Root, "blobs");
            LogDirectory = Path.Join(Root, "logs");
        }

        public string Root { get; }
        public string LockFile { get; }
        public string PidFile { get; }
        public string StateFile { get; }
        public string RuntimeConfigurationFile { get; }
        public string PasswordFile { get; }
        public string ApiKeyFile { get; }
        public string ShutdownFile { get; }
        public string BlobRoot { get; }
        public string LogDirectory { get; }

        public void EnsureCreated()
        {
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(Path.GetDirectoryName(PasswordFile)!);
            Directory.CreateDirectory(BlobRoot);
            Directory.CreateDirectory(LogDirectory);
        }
    }
}
