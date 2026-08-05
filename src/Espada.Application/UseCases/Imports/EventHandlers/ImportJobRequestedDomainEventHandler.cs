using Espada.Application.Contracts.Jobs;
using Espada.Domain.Enums;
using Espada.Domain.Events;
using Espada.Domain.SeedWork;

namespace Espada.Application.UseCases.Imports.EventHandlers
{
    internal sealed class ImportJobRequestedDomainEventHandler(IJobQueue jobQueue)
        : IDomainEventHandler<ImportJobRequestedDomainEvent>
    {
        public Task HandleAsync(
            ImportJobRequestedDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            return jobQueue.EnqueueAsync(domainEvent.ImportJobId, ImportPipelineStageType.Start,
                $"import:{domainEvent.ImportJobId.Value:N}:stage:{ImportPipelineStageType.Start}", cancellationToken);
        }
    }
}