namespace Espada.Tests.Domain.Assertions;

internal static class DomainEventAssertions
{
    extension(IHasDomainEvents source)
    {
        public TEvent ShouldHaveSingleDomainEvent<TEvent>() where TEvent : IDomainEvent
        {
            ArgumentNullException.ThrowIfNull(source);

            source.DomainEvents.Should().ContainSingle();

            IDomainEvent domainEvent = source.DomainEvents.Single();

            domainEvent.Should().BeOfType<TEvent>();

            return (TEvent)domainEvent;
        }

        public void ShouldHaveNoDomainEvents()
        {
            ArgumentNullException.ThrowIfNull(source);

            source.DomainEvents.Should().BeEmpty();
        }
    }
}