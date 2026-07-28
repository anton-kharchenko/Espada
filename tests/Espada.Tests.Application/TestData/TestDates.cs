namespace Espada.Tests.Application.TestData
{
    internal static class TestDates
    {
        public static readonly DateTimeOffset UtcNow = new(2026, 7, 24, 20, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset LaterUtc = new(2026, 7, 25, 20, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset WorkspaceArchivedAtUtc = new(2026, 7, 26, 18, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset SourceArchivedAtUtc = new(2026, 7, 27, 12, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ImportRequestedAtUtc = new(2026, 7, 28, 9, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ImportStartedAtUtc = new(2026, 7, 28, 9, 5, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ImportCompletedAtUtc = new(2026, 7, 28, 9, 5, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ImportFailedAtUtc = new(2026, 7, 28, 9, 20, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ImportCancelledAtUtc = new(2026, 7, 28, 9, 25, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArtifactCreatedAtUtc = new(2026, 7, 28, 10, 0, 0, TimeSpan.Zero);


        public static readonly DateTimeOffset ArtifactFirstRevisionCreatedAtUtc =
            new(2026, 7, 28, 10, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArtifactSecondRevisionCreatedAtUtc =
            new(2026, 7, 28, 10, 10, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArtifactRenamedAtUtc = new(2026, 7, 28, 10, 15, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArtifactArchivedAtUtc = new(2026, 7, 28, 10, 20, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ChunkBatchRequestedAtUtc = new(2026, 7, 29, 10, 0, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ChunkProcessingAtUtc = new(2026, 7, 29, 10, 5, 0, TimeSpan.Zero);
    }
}