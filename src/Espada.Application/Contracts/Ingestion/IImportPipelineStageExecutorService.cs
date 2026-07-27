using Espada.Application.Contracts.Jobs;

namespace Espada.Application.Contracts.Ingestion;

public interface IImportPipelineStageExecutorService
{
    Task ExecuteAsync(IngestionJob job, CancellationToken cancellationToken = default);
}