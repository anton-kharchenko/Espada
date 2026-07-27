namespace Espada.Domain.SeedWork;

public interface IDomainEventDispatcher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}