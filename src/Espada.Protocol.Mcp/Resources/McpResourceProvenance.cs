namespace Espada.Protocol.Mcp.Resources
{
    internal sealed record McpResourceProvenance(
        Guid WorkspaceId,
        Guid? ArtifactId = null,
        Guid? RevisionId = null,
        int? RevisionNumber = null,
        Guid? ChunkId = null);
}