using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record SourcePriorityChangedDomainEvent(SourceId SourceId, int PreviousPriority, int Priority, DateTimeOffset ChangedAtUtc) : IDomainEvent;