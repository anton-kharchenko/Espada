using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Domain.Enums;

namespace Espada.Tests.Application.TestData.Builder;

internal sealed class CreateWorkspaceCommandBuilder
{
    private string? _name = TestValues.WorkspaceName;

    private WorkspaceType _type = WorkspaceTypeTestData.Any;

    public CreateWorkspaceCommandBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    public CreateWorkspaceCommandBuilder WithType(WorkspaceType type)
    {
        _type = type;
        return this;
    }

    public CreateWorkspaceCommandBuilder WithoutType()
    {
        _type = null!;
        return this;
    }

    public CreateWorkspaceCommand Build() => new(_name!, _type);
}