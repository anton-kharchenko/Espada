using System.Text.Json.Serialization;

namespace Espada.Daemon.Runtime
{
    public sealed record LocalRuntimeState(
        int ProcessId,
        string Status,
        DateTimeOffset StartedAtUtc,
        int ApiPort,
        int McpPort,
        int PostgresPort,
        string PostgresContainerName,
        IReadOnlyDictionary<string, int> ChildProcessIds)
    {
        [JsonIgnore]
        public bool IsHealthy => string.Equals(Status, "healthy", StringComparison.Ordinal);
    }
}