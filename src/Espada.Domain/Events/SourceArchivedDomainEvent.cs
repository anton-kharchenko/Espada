using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record SourceArchivedDomainEvent(
    SourceId SourceId,
    DateTimeOffset ArchivedAtUtc) : IDomainEvent;