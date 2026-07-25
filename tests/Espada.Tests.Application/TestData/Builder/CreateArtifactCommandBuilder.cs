using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Domain.Enums;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class CreateArtifactCommandBuilder
    {
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        private string? _title = TestValues.ArtifactTitle;

        private int _typeId = ArtifactType.Markdown.Id;

        private string? _content = TestValues.ArtifactContent;

        public CreateArtifactCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public CreateArtifactCommandBuilder WithTitle(string? title)
        {
            _title = title;
            return this;
        }

        public CreateArtifactCommandBuilder WithType(int typeId)
        {
            _typeId = typeId;
            return this;
        }

        public CreateArtifactCommandBuilder WithContent(string? content)
        {
            _content = content;
            return this;
        }

        public CreateArtifactCommand Build() => new(_workspaceId, _title!, _typeId, _content!);
    }
}