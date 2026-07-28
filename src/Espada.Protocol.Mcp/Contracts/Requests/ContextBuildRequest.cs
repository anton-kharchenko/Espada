namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record ContextBuildRequest(
        Guid WorkspaceId,
        Guid? ProjectId,
        Guid? TaskId,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        int TokenBudget);
}