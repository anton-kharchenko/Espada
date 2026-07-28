using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ArtifactPriorityChangedDomainEvent(
        ArtifactId ArtifactId,
        int PreviousPriority,
        int Priority,
        DateTimeOffset ChangedAtUtc) : IDomainEvent;
}