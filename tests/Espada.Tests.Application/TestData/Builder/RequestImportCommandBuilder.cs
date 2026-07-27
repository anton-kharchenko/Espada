using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Tests.Application.TestData.Builder;

internal sealed class RequestImportCommandBuilder
{
    private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

    private Guid _sourceId = TestIds.SourceId.Value;
    private string _idempotencyKey = "request-import-test";
    private ImportOptions _options = new(EmbeddingModel: "test-embedding-model");

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
    
    public RequestImportCommand Build() => new(_workspaceId, _sourceId, _idempotencyKey, _options);
}