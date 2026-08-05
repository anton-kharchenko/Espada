namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record WorkspaceCreateRequest(
        string Name,
        int TypeId,
        Guid? OrganizationId = null);
}