namespace Espada.Domain.SeedWork;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    IReadOnlyCollection<IDomainEvent> DequeueDomainEvents();
}