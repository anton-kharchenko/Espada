using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ChunkBatchCompletedDomainEvent(
        ChunkBatchId ChunkBatchId,
        int ChunkCount,
        DateTimeOffset CompletedAtUtc) : IDomainEvent;
}