namespace Espada.Protocol.Mcp.Contracts.Requests;

public sealed record ArtifactGetRequest(
    Guid WorkspaceId,
    Guid ArtifactId);
