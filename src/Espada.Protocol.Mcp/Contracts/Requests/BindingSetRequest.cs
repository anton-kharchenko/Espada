namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record BindingSetRequest(
        Guid WorkspaceId,
        Guid ArtifactId,
        Guid? BindingId = null,
        Guid? OrganizationId = null,
        Guid? ProjectId = null,
        string? RepositoryCanonicalUri = null,
        string? RepositoryRelativePathPrefix = null,
        string? Branch = null,
        Guid? TaskId = null,
        string? Agent = null);
}