namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record MemoryRememberRequest(
        Guid WorkspaceId,
        string Title,
        string Content,
        int CategoryTypeId,
        decimal Confidence,
        string? SessionIdentity = null,
        Guid? SupersededMemoryId = null);
}