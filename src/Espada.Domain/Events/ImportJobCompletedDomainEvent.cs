using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ImportJobCompletedDomainEvent(
    ImportJobId ImportJobId,
    SourceId SourceId,
    ArtifactId ArtifactId,
    ArtifactRevisionId ArtifactRevisionId,
    DateTimeOffset CompletedAtUtc) : IDomainEvent;