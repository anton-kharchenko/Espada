namespace Espada.Cli.Models
{
    internal sealed record LocalRuntimeState(
        int ProcessId,
        string Status,
        DateTimeOffset StartedAtUtc,
        int ApiPort,
        int McpPort,
        int PostgresPort,
        string PostgresContainerName,
        IReadOnlyDictionary<string, int> ChildProcessIds);
}