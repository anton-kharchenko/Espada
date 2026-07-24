using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ImportJobRequestedDomainEvent(
    ImportJobId ImportJobId,
    SourceId SourceId,
    WorkspaceId WorkspaceId,
    DateTimeOffset RequestedAtUtc) : IDomainEvent;