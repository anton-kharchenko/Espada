using Espada.Db.Database;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Espada.Tests.Integration.Database
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class DatabaseMigrationTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task Database_AfterFixtureInitialization_ShouldBeAvailable()
        {
            await using SetupDbContext dbContext = Fixture.CreateSetupDbContext();

            bool canConnect = await dbContext.Database.CanConnectAsync(TestContext.Current.CancellationToken);

            Assert.True(canConnect);
        }

        [Fact]
        public async Task Database_AfterFixtureInitialization_ShouldHaveNoPendingMigrations()
        {
            await using SetupDbContext dbContext = Fixture.CreateSetupDbContext();

            string[] pendingMigrations =
                (await dbContext.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken)).ToArray();

            Assert.Empty(pendingMigrations);
        }

        [Fact]
        public void Model_ShouldHaveNoPendingChanges()
        {
            using SetupDbContext dbContext = Fixture.CreateSetupDbContext();

            Assert.False(dbContext.Database.HasPendingModelChanges());
        }

        [Fact]
        public async Task Database_AfterMigrations_ShouldAllowQueryingAllAggregateTables()
        {
            await using SetupDbContext dbContext = Fixture.CreateSetupDbContext();
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

        [Fact]
        public async Task ResetDatabase_ShouldDeleteDataAndPreserveMigrationHistory()
        {
            PersistenceGraph graph = PersistenceGraphFactory.Create();
            await using (EspadaDbContext setupContext = Fixture.CreateDbContext())
            {
                setupContext.Workspaces.Add(graph.Workspace);
                await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await Fixture.ResetDatabaseAsync();

            await using EspadaDbContext verificationContext = Fixture.CreateDbContext();
            Assert.Empty(await verificationContext.Workspaces.ToListAsync(TestContext.Current.CancellationToken));

            await using SetupDbContext migrationContext = Fixture.CreateSetupDbContext();
            Assert.NotEmpty(
                await migrationContext.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken));
            Assert.Empty(
                await migrationContext.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
        }
    }
}