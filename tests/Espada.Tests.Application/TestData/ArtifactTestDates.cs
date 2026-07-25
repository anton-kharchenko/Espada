namespace Espada.Tests.Application.TestData
{
    internal static class ArtifactTestDates
    {
        public static readonly DateTimeOffset CreatedAtUtc =
            new(2026, 7, 28, 10, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset FirstRevisionCreatedAtUtc =
            new(2026, 7, 28, 10, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset SecondRevisionCreatedAtUtc =
            new(2026, 7, 28, 10, 10, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArchivedAtUtc =
            new(2026, 7, 28, 10, 20, 0, TimeSpan.Zero);
    }
}