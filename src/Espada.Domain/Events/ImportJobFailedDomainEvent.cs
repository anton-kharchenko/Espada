using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ImportJobFailedDomainEvent(
        ImportJobId ImportJobId,
        string FailureCode,
        string FailureReason,
        DateTimeOffset FailedAtUtc) : IDomainEvent;
}