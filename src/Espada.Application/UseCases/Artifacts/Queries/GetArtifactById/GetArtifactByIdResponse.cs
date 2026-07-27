namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    public sealed record GetArtifactByIdResponse(
        Guid Id,
        Guid WorkspaceId,
        string Title,
        int TypeId,
        string TypeName,
        int StatusId,
        string StatusName,
        int Priority,
        Guid? CurrentRevisionId,
        int? CurrentRevisionNumber,
        int RevisionCount,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        DateTimeOffset? ArchivedAtUtc);
}