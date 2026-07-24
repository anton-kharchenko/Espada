namespace Espada.Application.Contracts.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}