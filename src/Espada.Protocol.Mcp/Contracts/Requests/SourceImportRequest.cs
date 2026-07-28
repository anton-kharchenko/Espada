namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record SourceImportRequest(
        Guid WorkspaceId,
        Guid SourceId,
        string IdempotencyKey,
        ImportOptionsRequest? Options = null);
}