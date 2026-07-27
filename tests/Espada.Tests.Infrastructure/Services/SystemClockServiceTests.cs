using Espada.Application.Contracts.Time;
using Espada.Infrastructure.Services;

namespace Espada.Tests.Infrastructure.Services
{
    public sealed class SystemClockServiceTests
    {
        [Fact]
        public void UtcNow_ShouldReturnCurrentUtcTime()
        {
            IClockService clockService = new SystemClockService();

            DateTimeOffset before = DateTimeOffset.UtcNow;
            DateTimeOffset result = clockService.UtcNow;
            DateTimeOffset after = DateTimeOffset.UtcNow;

            Assert.InRange(result, before, after);
            Assert.Equal(TimeSpan.Zero, result.Offset);
        }
    }
}