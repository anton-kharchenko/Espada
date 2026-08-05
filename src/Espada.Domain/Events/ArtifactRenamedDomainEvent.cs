using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ArtifactRenamedDomainEvent(
        ArtifactId ArtifactId,
        string PreviousTitle,
        string CurrentTitle,
        DateTimeOffset RenamedAtUtc) : IDomainEvent;
}