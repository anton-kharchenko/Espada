using Espada.Application.Contracts.Jobs;
using Espada.Domain.Events;
using Espada.Domain.SeedWork;

namespace Espada.Application.UseCases.Imports.EventHandlers
{
    internal sealed class ImportStageScheduledDomainEventHandler(IJobQueue jobQueue)
        : IDomainEventHandler<ImportStageScheduledDomainEvent>
    {
        public Task HandleAsync(
            ImportStageScheduledDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            return jobQueue.EnqueueAsync(domainEvent.ImportJobId, domainEvent.Stage,
                $"import:{domainEvent.ImportJobId.Value:N}:stage:{domainEvent.Stage}", cancellationToken);
        }
    }
}