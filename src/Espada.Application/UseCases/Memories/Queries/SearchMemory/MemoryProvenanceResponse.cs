namespace Espada.Application.UseCases.Memories.Queries.SearchMemory
{
    public sealed record MemoryProvenanceResponse(
        string ClientIdentity,
        string? SessionIdentity,
        DateTimeOffset CapturedAtUtc,
        bool UserConfirmed,
        Guid? SupersededMemoryId);
}