namespace Espada.Cli.Infrastructure
{
    internal sealed class LocalRuntimePaths
    {
        public LocalRuntimePaths()
        {
            string? configuredRoot = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT");
            Root = string.IsNullOrWhiteSpace(configuredRoot)
                ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada")
                : Path.GetFullPath(configuredRoot);
            StateFile = Path.Join(Root, "runtime-state.json");
            ApiKeyFile = Path.Join(Root, "secrets", "local-api-key");
            PasswordFile = Path.Join(Root, "secrets", "postgres-password");
        }

        public string Root { get; }
        public string StateFile { get; }
        public string ApiKeyFile { get; }

        public string PasswordFile { get; }
    }
}