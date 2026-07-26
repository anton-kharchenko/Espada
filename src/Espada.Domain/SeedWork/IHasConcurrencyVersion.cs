namespace Espada.Domain.SeedWork;

public interface IHasConcurrencyVersion
{
    long Version { get; }
}
