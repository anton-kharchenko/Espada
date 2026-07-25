using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class AddArtifactRevisionCommandBuilder
    {
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        private Guid _artifactId = ArtifactTestIds.DefaultArtifactId.Value;

        private string? _content = ArtifactTestValues.SecondContent;

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

        public AddArtifactRevisionCommand Build() =>
            new(
                _workspaceId,
                _artifactId,
                _content!);
    }
}