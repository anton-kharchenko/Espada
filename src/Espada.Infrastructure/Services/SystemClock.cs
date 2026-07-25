using Espada.Application.Contracts.Time;

namespace Espada.Infrastructure.Services
{
    internal sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}