namespace Espada.Tests.Domain.Assertions;

internal static class DomainEventAssertions
{
    public static TEvent ShouldHaveSingleDomainEvent<TEvent>(this IHasDomainEvents source) where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(source);

        source.DomainEvents.Should().ContainSingle();

        IDomainEvent domainEvent = source.DomainEvents.Single();

        domainEvent.Should().BeOfType<TEvent>();

        return (TEvent)domainEvent;
    }

    public static void ShouldHaveNoDomainEvents(this IHasDomainEvents source)
    {
        ArgumentNullException.ThrowIfNull(source);

        source.DomainEvents.Should().BeEmpty();
    }
}