namespace Espada.Tests.Domain.TestData
{
    internal static class TestDates
    {
        public static readonly DateTimeOffset CreatedAtUtc = new(2026, 7, 24, 10, 30, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset LaterUtc = new(2026, 7, 27, 15, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArchivedAtUtc = new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
        
        public static readonly DateTimeOffset ArtifactCreatedAtUtc = new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset ArtifactRenamedAtUtc = new(2026, 7, 25, 13, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset ArtifactArchivedAtUtc = new(2026, 7, 26, 14, 0, 0, TimeSpan.Zero);

        
        public static readonly DateTimeOffset FirstRevisionCreatedAtUtc = new(2026, 7, 27, 15, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset SecondRevisionCreatedAtUtc = new(2026, 7, 28, 16, 0, 0, TimeSpan.Zero);
        
        public static readonly DateTimeOffset SourceCreatedAtUtc = new(2026, 7, 24, 10, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset SourceArchivedAtUtc = new(2026, 7, 25, 10, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ImportRequestedAtUtc = new(2026, 7, 26, 10, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset ImportStartedAtUtc = new(2026, 7, 26, 10, 5, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset ImportCompletedAtUtc = new(2026, 7, 26, 10, 10, 0, TimeSpan.Zero);
    }
}