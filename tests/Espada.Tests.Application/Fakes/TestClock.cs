using Espada.Application.Contracts.Time;

namespace Espada.Tests.Application.Fakes;

internal sealed class TestClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}