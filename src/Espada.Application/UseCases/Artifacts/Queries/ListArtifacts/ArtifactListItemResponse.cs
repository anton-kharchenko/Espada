namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    public sealed record ArtifactListItemResponse(
        Guid Id,
        string Title,
        int KindTypeId,
        string KindTypeName,
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