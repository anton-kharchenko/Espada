using Espada.Application.UseCases.Imports.Commands.CompleteImport;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class CompleteImportCommandBuilder
    {
        private Guid _artifactId = TestIds.DefaultArtifactId.Value;

        private Guid _artifactRevisionId = TestIds.DefaultArtifactRevisionId.Value;

        private Guid _importJobId = TestIds.DefaultImportJobId.Value;
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        public CompleteImportCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public CompleteImportCommandBuilder ForImportJob(Guid importJobId)
        {
            _importJobId = importJobId;
            return this;
        }

        public CompleteImportCommandBuilder WithArtifact(Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public CompleteImportCommandBuilder WithArtifactRevision(Guid artifactRevisionId)
        {
            _artifactRevisionId = artifactRevisionId;
            return this;
        }

        public CompleteImportCommand Build()
        {
            return new CompleteImportCommand(
                _workspaceId,
                _importJobId,
                _artifactId,
                _artifactRevisionId);
        }
    }
}