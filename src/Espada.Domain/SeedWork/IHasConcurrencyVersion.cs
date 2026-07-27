namespace Espada.Domain.SeedWork;

public interface IHasConcurrencyVersion
{
    uint Version { get; }
}