namespace Espada.Application.Contracts.Time;

public interface IClockService
{
    DateTimeOffset UtcNow { get; }
}