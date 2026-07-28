using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ChunkBatchFailedDomainEvent(
        ChunkBatchId ChunkBatchId,
        string Reason,
        DateTimeOffset FailedAtUtc) : IDomainEvent;
}