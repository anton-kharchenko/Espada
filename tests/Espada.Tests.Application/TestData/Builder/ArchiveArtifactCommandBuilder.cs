using Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class ArchiveArtifactCommandBuilder
    {
        private Guid _artifactId =
            TestIds.DefaultArtifactId.Value;

        private Guid _workspaceId =
            TestIds.DefaultWorkspaceId.Value;

        public ArchiveArtifactCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ArchiveArtifactCommandBuilder ForArtifact(Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public ArchiveArtifactCommand Build()
        {
            return new ArchiveArtifactCommand(
                _workspaceId,
                _artifactId);
        }
    }
}