using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record WorkspaceArchivedDomainEvent(WorkspaceId WorkspaceId, DateTimeOffset ArchivedAtUtc) : IDomainEvent;
}