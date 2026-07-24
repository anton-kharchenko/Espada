using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ImportJobStartedDomainEvent(
    ImportJobId ImportJobId,
    DateTimeOffset StartedAtUtc) : IDomainEvent;