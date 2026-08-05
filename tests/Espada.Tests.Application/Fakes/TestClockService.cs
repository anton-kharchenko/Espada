using Espada.Application.Contracts.Time;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class TestClockService(DateTimeOffset utcNow) : IClockService
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;
    }
}