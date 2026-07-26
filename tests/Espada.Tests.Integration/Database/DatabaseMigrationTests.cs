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
    public void Model_ShouldHaveNoPendingChanges()
    {
        using EspadaDbContext dbContext = fixture.CreateDbContext();

        Assert.False(dbContext.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task Database_AfterMigrations_ShouldAllowQueryingAllAggregateTables()
    {
        await using EspadaDbContext dbContext = fixture.CreateDbContext();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        int[] tableRowCounts =
        [
            await dbContext.Workspaces.CountAsync(cancellationToken),
            await dbContext.Sources.CountAsync(cancellationToken),
            await dbContext.ImportJobs.CountAsync(cancellationToken),
            await dbContext.Artifacts.CountAsync(cancellationToken),
            await dbContext.ArtifactRevisions.CountAsync(cancellationToken),
            await dbContext.ChunkBatches.CountAsync(cancellationToken),
            await dbContext.Chunks.CountAsync(cancellationToken),
            await dbContext.ChunkEmbeddings.CountAsync(cancellationToken)
        ];

        Assert.All(tableRowCounts, count => Assert.True(count >= 0));
    }
}