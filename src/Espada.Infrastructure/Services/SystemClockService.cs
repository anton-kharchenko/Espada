using Espada.Application.Contracts.Time;

namespace Espada.Infrastructure.Services
{
    internal sealed class SystemClockService : IClockService
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}