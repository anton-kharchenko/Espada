using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ArtifactRevisionCreatedDomainEvent(
    ArtifactId ArtifactId,
    ArtifactRevisionId RevisionId,
    int RevisionNumber,
    string ContentHash,
    int SizeInBytes,
    DateTimeOffset CreatedAtUtc) : IDomainEvent;