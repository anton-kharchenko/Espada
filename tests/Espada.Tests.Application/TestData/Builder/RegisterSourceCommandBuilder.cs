using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Domain.Enums;

namespace Espada.Tests.Application.TestData.Builder;

internal sealed class RegisterSourceCommandBuilder
{
    private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

    private string _name = TestValues.SourceName;

    private string _locator = TestValues.SourceLocator;

    private SourceType _type = SourceTypeTestData.Any;

    public RegisterSourceCommandBuilder InWorkspace(Guid workspaceId)
    {
        _workspaceId = workspaceId;
        return this;
    }

    public RegisterSourceCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public RegisterSourceCommandBuilder WithLocator(string locator)
    {
        _locator = locator;
        return this;
    }

    public RegisterSourceCommandBuilder WithType(SourceType type)
    {
        _type = type;
        return this;
    }

    public RegisterSourceCommandBuilder WithoutType()
    {
        _type = null!;
        return this;
    }

    public RegisterSourceCommand Build() => new(_workspaceId, _name, _locator, _type);
}