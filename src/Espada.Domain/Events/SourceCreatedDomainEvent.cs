using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record SourceCreatedDomainEvent(
        SourceId SourceId,
        WorkspaceId WorkspaceId,
        string Name,
        SourceType Type,
        string Locator,
        DateTimeOffset CreatedAtUtc) : IDomainEvent;
}