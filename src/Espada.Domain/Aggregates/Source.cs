using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates;

public sealed class Source : AggregateRoot<SourceId>, IHasConcurrencyVersion
{
    public long Version { get; private set; } = 1;

    private Source()
    {
    }

    private Source(
        SourceId id,
        WorkspaceId workspaceId,
        SourceName name,
        SourceType type,
        SourceLocator locator,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        WorkspaceId = workspaceId;
        Name = name;
        Type = type;
        Locator = locator;
        Status = SourceStatusType.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public WorkspaceId WorkspaceId { get; private set; } = null!;

    public SourceName Name { get; private set; } = null!;

    public SourceType Type { get; private set; } = null!;

    public SourceLocator Locator { get; private set; } = null!;

    public SourceStatusType Status { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static DomainResult<Source> Create(
        SourceId id,
        WorkspaceId workspaceId,
        SourceName name,
        SourceType type,
        SourceLocator locator,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(locator);

        Source source = new(id, workspaceId, name, type, locator, createdAtUtc);

        source.RaiseDomainEvent(new SourceCreatedDomainEvent(source.Id, source.WorkspaceId, source.Name.Value, source.Type, source.Locator.Value, createdAtUtc));

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
}