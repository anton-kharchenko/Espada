namespace Espada.Domain.SeedWork;

public interface IDomainEventDispatcher
{
    void Publish(IDomainEvent domainEvent);
}
