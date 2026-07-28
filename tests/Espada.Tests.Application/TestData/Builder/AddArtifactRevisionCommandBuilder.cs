using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class AddArtifactRevisionCommandBuilder
    {
        private Guid _artifactId = TestIds.DefaultArtifactId.Value;

        private string? _content = TestValues.AnotherArtifactContent;
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        public AddArtifactRevisionCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public AddArtifactRevisionCommandBuilder ForArtifact(Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public AddArtifactRevisionCommandBuilder WithContent(string? content)
        {
            _content = content;
            return this;
        }

        public AddArtifactRevisionCommand Build()
        {
            return new AddArtifactRevisionCommand(
                _workspaceId,
                _artifactId,
                _content!);
        }
    }
}