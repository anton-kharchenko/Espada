using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class RequestImportCommandBuilder
    {
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        private Guid _sourceId = TestIds.SourceId.Value;

        public RequestImportCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public RequestImportCommandBuilder ForSource(Guid sourceId)
        {
            _sourceId = sourceId;
            return this;
        }

        public RequestImportCommand Build() => new(_workspaceId, _sourceId);
    }
}