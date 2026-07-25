using Espada.Application.UseCases.Imports.Commands.CancelImport;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class CancelImportCommandBuilder
    {
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        private Guid _importJobId = TestIds.DefaultImportJobId.Value;

        public CancelImportCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public CancelImportCommandBuilder ForImportJob(Guid importJobId)
        {
            _importJobId = importJobId;
            return this;
        }

        public CancelImportCommand Build() =>
            new(
                _workspaceId,
                _importJobId);
    }
}