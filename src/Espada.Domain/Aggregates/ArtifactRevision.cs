using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates;

public sealed class ArtifactRevision : AggregateRoot<ArtifactRevisionId>
{
    private ArtifactRevision()
    {
    }

    private ArtifactRevision(
        ArtifactRevisionId id,
        ArtifactId artifactId,
        WorkspaceId workspaceId,
        ArtifactKindType kindType,
        RevisionNumber number,
        ArtifactContent content,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        ArtifactId = artifactId;
        WorkspaceId = workspaceId;
        KindType = kindType;
        Number = number;
        Content = content;
        CreatedAtUtc = createdAtUtc;
    }

    public ArtifactId ArtifactId { get; private set; } = null!;

    public WorkspaceId WorkspaceId { get; private set; } = null!;

    public ArtifactKindType KindType { get; private set; } = null!;

    public RevisionNumber Number { get; private set; } = null!;

    public ArtifactContent Content { get; private set; } = null!;

    public ContentHash ContentHash => Content.Hash;

    public int SizeInBytes => Content.SizeInBytes;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal static ArtifactRevision Create(
        ArtifactRevisionId id,
        ArtifactId artifactId,
        WorkspaceId workspaceId,
        ArtifactKindType kindType,
        RevisionNumber number,
        ArtifactContent content,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(artifactId);
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(kindType);
        ArgumentNullException.ThrowIfNull(number);
        ArgumentNullException.ThrowIfNull(content);

        return new ArtifactRevision(id, artifactId, workspaceId, kindType, number, content, createdAtUtc);
    }
}
