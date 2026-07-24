using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ArtifactArchivedDomainEvent(
    ArtifactId ArtifactId,
    DateTimeOffset ArchivedAtUtc) : IDomainEvent;