namespace Espada.Application.UseCases.Imports.Queries.ListImports
{
    public sealed record ImportListItemResponse(
        Guid Id,
        Guid SourceId,
        Guid WorkspaceId,
        int StatusId,
        string StatusName,
        string Stage,
        DateTimeOffset RequestedAtUtc,
        DateTimeOffset? StartedAtUtc,
        DateTimeOffset? CompletedAtUtc,
        Guid? ArtifactId,
        Guid? ArtifactRevisionId,
        string? FailureCode,
        string? FailureReason);
}
