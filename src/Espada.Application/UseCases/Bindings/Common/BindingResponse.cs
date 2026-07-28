namespace Espada.Application.UseCases.Bindings.Common
{
    public sealed record BindingResponse(
        Guid Id,
        Guid ArtifactRevisionId,
        Guid WorkspaceId,
        Guid? OrganizationId,
        Guid? ProjectId,
        string? RepositoryCanonicalUri,
        string? RepositoryRelativePathPrefix,
        string? Branch,
        Guid? TaskId,
        string? Agent,
        DateTimeOffset CreatedAtUtc);
}