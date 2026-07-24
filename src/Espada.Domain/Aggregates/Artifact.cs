using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates;

public sealed class Artifact : AggregateRoot<ArtifactId>
{
    private Artifact()
    {
    }

    private Artifact(
        ArtifactId id,
        WorkspaceId workspaceId,
        ArtifactTitle title,
        ArtifactType type,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        WorkspaceId = workspaceId;
        Title = title;
        Type = type;
        Status = ArtifactStatusType.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public WorkspaceId WorkspaceId { get; private set; } = null!;

    public ArtifactTitle Title { get; private set; } = null!;

    public ArtifactType Type { get; private set; }

    public ArtifactStatusType Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static DomainResult<Artifact> Create(
        ArtifactId id,
        WorkspaceId workspaceId,
        ArtifactTitle title,
        ArtifactType type,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(title);

        Artifact artifact = new(id, workspaceId, title, type, createdAtUtc);

        artifact.RaiseDomainEvent(new ArtifactCreatedDomainEvent(artifact.Id, artifact.WorkspaceId, artifact.Title.Value, artifact.Type, createdAtUtc));

        return DomainResult<Artifact>.Success(artifact);
    }

    public DomainResult Rename(ArtifactTitle title, DateTimeOffset renamedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(title);

        if (Equals(Status, ArtifactStatusType.Archived))
        {
            return DomainResult.Failure(ArtifactErrors.ArchivedArtifactCannotBeRenamed);
        }

        if (Title == title)
        {
            return DomainResult.Success();
        }

        string previousTitle = Title.Value;

        Title = title;
        UpdatedAtUtc = renamedAtUtc;

        RaiseDomainEvent(new ArtifactRenamedDomainEvent(Id, previousTitle, title.Value, renamedAtUtc));

        return DomainResult.Success();
    }

    public DomainResult Archive(DateTimeOffset archivedAtUtc)
    {
        if (Equals(Status, ArtifactStatusType.Archived))
        {
            return DomainResult.Failure(ArtifactErrors.AlreadyArchived);
        }

        Status = ArtifactStatusType.Archived;
        ArchivedAtUtc = archivedAtUtc;
        UpdatedAtUtc = archivedAtUtc;

        RaiseDomainEvent(new ArtifactArchivedDomainEvent(Id, archivedAtUtc));
        
        return DomainResult.Success();
    }
}