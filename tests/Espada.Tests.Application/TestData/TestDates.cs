namespace Espada.Tests.Application.TestData;

internal static class TestDates
{
    public static readonly DateTimeOffset UtcNow = new(2026, 7, 24, 20, 0, 0, TimeSpan.Zero);

    public static readonly DateTimeOffset LaterUtc = new(2026, 7, 25, 20, 0, 0, TimeSpan.Zero);
}