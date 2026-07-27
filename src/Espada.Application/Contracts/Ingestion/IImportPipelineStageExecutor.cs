using Espada.Application.Contracts.Jobs;

namespace Espada.Application.Contracts.Ingestion;

public interface IImportPipelineStageExecutor
{
    Task ExecuteAsync(IngestionJob job, CancellationToken cancellationToken = default);
}