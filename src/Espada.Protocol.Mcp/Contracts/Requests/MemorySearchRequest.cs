namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record MemorySearchRequest(
        Guid WorkspaceId,
        string QueryText,
        IReadOnlyCollection<int>? CategoryTypeIds = null,
        int TopK = 10);
}