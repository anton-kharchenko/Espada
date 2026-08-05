namespace Espada.Application.UseCases.Memories.Commands.RememberMemory
{
    public sealed record RememberMemoryResponse(
        Guid MemoryId,
        Guid ArtifactId,
        Guid RevisionId,
        bool UserConfirmed,
        DateTimeOffset CapturedAtUtc);
}