namespace Espada.Tests.Domain.TestData.Builders;

internal sealed class WorkspaceBuilder
{
    private WorkspaceId _id = TestIds.WorkspaceId;

    private WorkspaceName _name = CreateName("Espada Workspace");

    private WorkspaceType _type = WorkspaceType.Personal;

    private DateTimeOffset _createdAtUtc = new(2026, 7, 24, 10, 30, 0, TimeSpan.Zero);

    public WorkspaceBuilder WithId(WorkspaceId id)
    {
        _id = id;
        return this;
    }

    public WorkspaceBuilder WithName(string name)
    {
        _name = CreateName(name);
        return this;
    }

    public WorkspaceBuilder WithName(WorkspaceName name)
    {
        _name = name;
        return this;
    }

    public WorkspaceBuilder WithType(WorkspaceType type)
    {
        _type = type;
        return this;
    }

    public WorkspaceBuilder CreatedAt(DateTimeOffset createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public DomainResult<Workspace> BuildResult() => Workspace.Create(_id, _name, _type, _createdAtUtc);

    public Workspace Build()
    {
        DomainResult<Workspace> result = BuildResult();

        return result.IsFailure ? throw new InvalidOperationException($"WorkspaceBuilder produced an invalid workspace: {result.Error.Code} — {result.Error.Description}") : result.Value;
    }

    private static WorkspaceName CreateName(string value)
    {
        DomainResult<WorkspaceName> result = WorkspaceName.Create(value);

        return result.IsFailure ? throw new InvalidOperationException($"WorkspaceBuilder received an invalid name: " + $"{result.Error.Code} — {result.Error.Description}") : result.Value;
    }
}