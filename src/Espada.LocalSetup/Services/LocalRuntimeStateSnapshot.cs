namespace Espada.LocalSetup.Services
{
    internal sealed record LocalRuntimeStateSnapshot(int ApiPort, int McpPort, int PostgresPort);
}