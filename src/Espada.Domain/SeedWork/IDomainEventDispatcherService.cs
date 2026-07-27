namespace Espada.Domain.SeedWork;

public interface IDomainEventDispatcherService
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}