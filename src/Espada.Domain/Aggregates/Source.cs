using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Domain.Aggregates;

public sealed class Source : AggregateRoot<SourceId>, IHasConcurrencyVersion
{
    private SourceDefinition? _definition;

    public uint Version { get; private set; }

    private Source()
    {
    }

    private Source(
        SourceId id,
        WorkspaceId workspaceId,
        SourceName name,
        SourceType type,
        SourceLocator locator,
        SourceDefinition definition,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        WorkspaceId = workspaceId;
        Name = name;
        Type = type;
        Locator = locator;
        _definition = definition;
        Status = SourceStatusType.Active;
        Priority = ContextPriority.Neutral;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public WorkspaceId WorkspaceId { get; private set; } = null!;

    public SourceName Name { get; private set; } = null!;

    public SourceType Type { get; private set; } = null!;

    public SourceLocator Locator { get; private set; } = null!;

    public SourceDefinition Definition => _definition ?? new LegacySourceDefinition(Type.Id, Locator.Value);

    public SourceStatusType Status { get; private set; } = null!;

    public ContextPriority Priority { get; private set; } = ContextPriority.Neutral;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static DomainResult<Source> Create(SourceId id, WorkspaceId workspaceId, SourceName name, SourceType type, SourceLocator locator, DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(locator);

        SourceDefinition definition = new LegacySourceDefinition(type.Id, locator.Value);
        Source source = new(id, workspaceId, name, type, locator, definition, createdAtUtc);
        source.RaiseDomainEvent(new SourceCreatedDomainEvent(source.Id, source.WorkspaceId, source.Name.Value, source.Type, source.Locator.Value, source.Definition, createdAtUtc));

        return DomainResult<Source>.Success(source);
    }

    public static DomainResult<Source> Create(SourceId id, WorkspaceId workspaceId, SourceName name, SourceDefinition definition, DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(definition);

        DomainResult<SourceLocator> locator = SourceLocator.Create(definition.CanonicalLocator);
        if (locator.IsFailure)
        {
            return DomainResult.Failure<Source>(locator.Error);
        }

        Source source = new(id, workspaceId, name, definition.SourceType, locator.Value, definition, createdAtUtc);
        source.RaiseDomainEvent(new SourceCreatedDomainEvent(source.Id, source.WorkspaceId, source.Name.Value, source.Type, source.Locator.Value, source.Definition, createdAtUtc));

        return DomainResult<Source>.Success(source);
    }

    public DomainResult Archive(DateTimeOffset archivedAtUtc)
    {
        if (Status.Equals(SourceStatusType.Archived))
        {
            return DomainResult.Failure(SourceErrors.AlreadyArchived);
        }

        Status = SourceStatusType.Archived;
        ArchivedAtUtc = archivedAtUtc;
        UpdatedAtUtc = archivedAtUtc;

        RaiseDomainEvent(new SourceArchivedDomainEvent(Id, archivedAtUtc));

        return DomainResult.Success();
    }

    public DomainResult SetPriority(ContextPriority priority, DateTimeOffset changedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(priority);

        if (Status.Equals(SourceStatusType.Archived))
        {
            return DomainResult.Failure(SourceErrors.ArchivedSourceCannotChangePriority);
        }

        if (Priority == priority)
        {
            return DomainResult.Success();
        }

        int previousPriority = Priority.Value;
        Priority = priority;
        UpdatedAtUtc = changedAtUtc;

        RaiseDomainEvent(new SourcePriorityChangedDomainEvent(Id, previousPriority, priority.Value, changedAtUtc));

        return DomainResult.Success();
    }
}