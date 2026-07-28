namespace Espada.Tests.Domain.TestData.Builders;

internal sealed class ArtifactBuilder
{
    private ArtifactId _id = TestIds.DefaultArtifactId;

    private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;

    private ArtifactTitle _title = ArtifactTitle.Create("Espada artifact").ShouldSucceed();

    private ArtifactType _type = ArtifactType.Markdown;

    private DateTimeOffset _createdAtUtc = TestDates.ArtifactCreatedAtUtc;

    public ArtifactBuilder WithId(ArtifactId id)
    {
        _id = id;
        return this;
    }

    public ArtifactBuilder InWorkspace(WorkspaceId workspaceId)
    {
        _workspaceId = workspaceId;
        return this;
    }

    public ArtifactBuilder WithTitle(string title)
    {
        _title = ArtifactTitle.Create(title).ShouldSucceed();
        return this;
    }

    public ArtifactBuilder WithTitle(ArtifactTitle title)
    {
        _title = title;
        return this;
    }

    public ArtifactBuilder WithType(ArtifactType type)
    {
        _type = type;
        return this;
    }

    public ArtifactBuilder CreatedAt(DateTimeOffset createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public Artifact Build() => Artifact.Create(_id, _workspaceId, _title, ArtifactKindType.Document, _type, _createdAtUtc).ShouldSucceed();
}