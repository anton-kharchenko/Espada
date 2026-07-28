namespace Espada.Application.UseCases.Memories.Queries.SearchMemory
{
    public sealed record MemorySearchItemResponse(
        Guid MemoryId,
        Guid ArtifactId,
        Guid RevisionId,
        string Title,
        string Content,
        int CategoryTypeId,
        string CategoryTypeName,
        decimal Confidence,
        double Score,
        MemoryProvenanceResponse Provenance);
}