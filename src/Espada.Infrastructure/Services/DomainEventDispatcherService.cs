using Espada.Domain.Events;
using Espada.Domain.SeedWork;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Infrastructure.Services;

internal sealed class DomainEventDispatcherService(IServiceProvider serviceProvider) : IDomainEventDispatcherService
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return domainEvent switch
        {
            ImportJobRequestedDomainEvent requested => DispatchAsync(requested, cancellationToken),
            ImportStageScheduledDomainEvent scheduled => DispatchAsync(scheduled, cancellationToken),
            _ => Task.CompletedTask
        };
    }

    private async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken) where TEvent : IDomainEvent
    {
        IEnumerable<IDomainEventHandler<TEvent>> handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        foreach (IDomainEventHandler<TEvent> handler in handlers)
        {
            await handler.HandleAsync(domainEvent, cancellationToken);
        }
    }
}