using Espada.Application.Contracts.Time;
using Espada.Infrastructure.Services;

namespace Espada.Tests.Infrastructure.Services
{
    public sealed class SystemClockTests
    {
        [Fact]
        public void UtcNow_ShouldReturnCurrentUtcTime()
        {
            IClock clock = new SystemClock();

            DateTimeOffset before = DateTimeOffset.UtcNow;
            DateTimeOffset result = clock.UtcNow;
            DateTimeOffset after = DateTimeOffset.UtcNow;

            Assert.InRange(result, before, after);
            Assert.Equal(TimeSpan.Zero, result.Offset);
        }
    }
}