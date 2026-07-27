using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Tests.Application.TestData.Builder;

internal sealed class RegisterSourceCommandBuilder
{
    private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

    private string? _name = TestValues.SourceName;

    private SourceDefinition? _definition =
        new LegacySourceDefinition(SourceTypeTestData.Any.Id, TestValues.SourceLocator);

    public RegisterSourceCommandBuilder InWorkspace(Guid workspaceId)
    {
        _workspaceId = workspaceId;
        return this;
    }

    public RegisterSourceCommandBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    public RegisterSourceCommandBuilder WithLocator(string? locator)
    {
        _definition = new LegacySourceDefinition(SourceTypeTestData.Any.Id, locator!);
        return this;
    }

    public RegisterSourceCommandBuilder WithType(SourceType type)
    {
        _definition = new LegacySourceDefinition(type.Id, TestValues.SourceLocator);
        return this;
    }

    public RegisterSourceCommandBuilder WithoutType()
    {
        _definition = null;
        return this;
    }

    public RegisterSourceCommand Build() => new(_workspaceId, _name!, _definition!);
}