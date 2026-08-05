namespace Espada.Application.UseCases.Search.Queries.UnifiedSearch
{
    public sealed record UnifiedSearchItemResponse(
        string HitType,
        Guid ChunkId,
        Guid ArtifactId,
        Guid RevisionId,
        Guid? SourceId,
        int? SourceTypeId,
        string ArtifactKind,
        int ArtifactTypeId,
        string Title,
        string Content,
        int? SourceSpanStart,
        int? SourceSpanLength,
        double Score,
        string Provenance);
}
