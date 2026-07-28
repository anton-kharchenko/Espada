using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Application.Constants;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class RegisterSourceCommandBuilder
    {
        private SourceDefinition? _definition = new FileSourceDefinition(TestValues.SourceLocator, null,
            Path.GetFileName(new Uri(TestValues.SourceLocator).LocalPath), IngestionMediaTypeConstants.Markdown);

        private string? _name = TestValues.SourceName;
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

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

        public RegisterSourceCommand Build()
        {
            return new RegisterSourceCommand(_workspaceId, _name!, _definition!);
        }
    }
}