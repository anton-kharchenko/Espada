namespace Espada.Tests.Domain.Assertions;

internal static class DomainEventAssertions
{
    public static TEvent ShouldHaveSingleDomainEvent<TEvent>(this IHasDomainEvents source) where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(source);

        source.DomainEvents.Should().ContainSingle(domainEvent => domainEvent is TEvent);

        return source.DomainEvents.OfType<TEvent>().Single();
    }

    public static void ShouldHaveNoDomainEvents(this IHasDomainEvents source)
    {
        ArgumentNullException.ThrowIfNull(source);

        source.DomainEvents.Should().BeEmpty();
    }
}