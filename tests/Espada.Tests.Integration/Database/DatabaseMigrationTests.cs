using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Espada.Tests.Integration.Database;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class DatabaseMigrationTests(PostgreSqlDatabaseFixture fixture)
{
    [Fact]
    public async Task Database_AfterFixtureInitialization_ShouldBeAvailable()
    {
        await using EspadaDbContext dbContext = fixture.CreateDbContext();

        bool canConnect = await dbContext.Database.CanConnectAsync(TestContext.Current.CancellationToken);

        Assert.True(canConnect);
    }

    [Fact]
    public async Task Database_AfterFixtureInitialization_ShouldHaveNoPendingMigrations()
    {
        await using EspadaDbContext dbContext = fixture.CreateDbContext();

        string[] pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken: TestContext.Current.CancellationToken)).ToArray();

        Assert.Empty(pendingMigrations);
    }

    [Fact]
    public async Task Database_AfterMigrations_ShouldContainExpectedTables()
    {
        await using EspadaDbContext dbContext = fixture.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken: TestContext.Current.CancellationToken);

        await using System.Data.Common.DbCommand command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'Espada'
            ORDER BY table_name;
            """;

        HashSet<string> actualTables = new(StringComparer.Ordinal);

        await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            actualTables.Add(reader.GetString(0));
        }

        string[] expectedTables =
        [
            "ArtifactRevisions",
            "Artifacts",
            "ChunkBatches",
            "ChunkEmbeddings",
            "Chunks",
            "ImportJobs",
            "Sources",
            "Workspaces"
        ];

        foreach (string expectedTable in expectedTables)
        {
            Assert.Contains(expectedTable, actualTables);
        }
    }
}