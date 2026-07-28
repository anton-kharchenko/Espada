using Espada.Application.UseCases.Imports.Queries.GetImportById;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class GetImportByIdQueryBuilder
    {
        private Guid _importJobId = TestIds.DefaultImportJobId.Value;
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        public GetImportByIdQueryBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public GetImportByIdQueryBuilder ForImportJob(Guid importJobId)
        {
            _importJobId = importJobId;
            return this;
        }

        public GetImportByIdQuery Build()
        {
            return new GetImportByIdQuery(
                _workspaceId,
                _importJobId);
        }
    }
}