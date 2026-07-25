using Espada.Application.UseCases.Artifacts.Commands.RenameArtifact;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class RenameArtifactCommandBuilder
    {
        private Guid _workspaceId =
            TestIds.DefaultWorkspaceId.Value;

        private Guid _artifactId =
            TestIds.DefaultArtifactId.Value;

        private string? _title =
            TestValues.RenamedArtifactTitle;

        public RenameArtifactCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public RenameArtifactCommandBuilder ForArtifact(Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public RenameArtifactCommandBuilder WithTitle(string? title)
        {
            _title = title;
            return this;
        }

        public RenameArtifactCommand Build()
        {
            return new RenameArtifactCommand(
                _workspaceId,
                _artifactId,
                _title!);
        }
    }
}