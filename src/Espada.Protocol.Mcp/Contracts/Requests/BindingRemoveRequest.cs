namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record BindingRemoveRequest(
        Guid WorkspaceId,
        Guid BindingId);
}