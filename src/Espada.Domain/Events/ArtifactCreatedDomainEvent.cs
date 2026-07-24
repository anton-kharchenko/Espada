using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ArtifactCreatedDomainEvent(
    ArtifactId ArtifactId,
    WorkspaceId WorkspaceId,
    string Title,
    ArtifactType Type,
    DateTimeOffset CreatedAtUtc) : IDomainEvent;