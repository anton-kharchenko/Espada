namespace Espada.Daemon.Runtime
{
    public sealed class LocalRuntimeOptions
    {
        public const string SectionName = "Espada:LocalRuntime";

        public bool Enabled { get; set; } = true;
        public string? DataRoot { get; set; }
        public string DockerExecutable { get; set; } = "docker";
        public string PostgresImage { get; set; } = "pgvector/pgvector:0.8.2-pg17";
        public string PostgresContainerName { get; set; } = "espada-postgres";
        public string PostgresVolumeName { get; set; } = "espada-postgres-data";
        public int ApiPort { get; set; } = 7432;
        public int McpPort { get; set; } = 7433;
        public int PostgresPort { get; set; } = 5433;
        public string? ApiExecutable { get; set; }
        public string? McpExecutable { get; set; }
        public string? WorkerExecutable { get; set; }
        public string? DbExecutable { get; set; }
        public int StartupTimeoutSeconds { get; set; } = 60;
        public int ShutdownTimeoutSeconds { get; set; } = 15;
    }
}
