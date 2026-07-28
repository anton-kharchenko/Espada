using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class RequestImportCommandBuilder
    {
        private string _idempotencyKey = "request-import-test";
        private ImportOptions _options = new("test-embedding-model");

        private Guid _sourceId = TestIds.SourceId.Value;
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

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

        public RequestImportCommandBuilder WithIdempotencyKey(string idempotencyKey)
        {
            _idempotencyKey = idempotencyKey;
            return this;
        }


        public RequestImportCommandBuilder WithEmbeddingModel(string? embeddingModel)
        {
            _options = _options with { EmbeddingModel = embeddingModel };
            return this;
        }

        public RequestImportCommand Build()
        {
            return new RequestImportCommand(_workspaceId, _sourceId, _idempotencyKey, _options);
        }
    }
}