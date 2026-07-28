using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ChunkBatchStartedDomainEvent(ChunkBatchId ChunkBatchId, DateTimeOffset StartedAtUtc)
        : IDomainEvent;
}