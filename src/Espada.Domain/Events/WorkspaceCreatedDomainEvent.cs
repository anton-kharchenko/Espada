using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record WorkspaceCreatedDomainEvent(
        WorkspaceId WorkspaceId,
        string Name,
        DateTimeOffset CreatedAtUtc) : IDomainEvent;
}