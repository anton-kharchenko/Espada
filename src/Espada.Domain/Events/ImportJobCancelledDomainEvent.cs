using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ImportJobCancelledDomainEvent(
        ImportJobId ImportJobId,
        DateTimeOffset CancelledAtUtc) : IDomainEvent;
}