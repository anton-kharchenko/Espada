using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ImportStageScheduledDomainEvent(
    ImportJobId ImportJobId,
    ImportPipelineStageType Stage,
    DateTimeOffset ScheduledAtUtc) : IDomainEvent;