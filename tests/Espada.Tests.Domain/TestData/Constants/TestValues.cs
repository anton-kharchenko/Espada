namespace Espada.Tests.Domain.TestData.Constants
{
    internal static class TestValues
    {
        public const string WorkspaceName = "Espada Workspace";
        public const string RenamedWorkspaceName = "Espada Team";

        public static readonly DateTimeOffset CreatedAtUtc = new(2026, 7, 24, 10, 30, 0, TimeSpan.Zero);

        public static readonly DateTimeOffset ArchivedAtUtc = new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
    }
}