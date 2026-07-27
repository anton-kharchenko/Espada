using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Espada.Tests.Integration.Transactions;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class TransactionTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
{
    [Fact]
    public async Task ArtifactAndFirstRevision_WhenRevisionFails_ShouldPersistNeither()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using EspadaDbContext setupContext = Fixture.CreateDbContext();
        setupContext.Workspaces.Add(graph.Workspace);
        await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Artifact artifact = Artifact.Create(ArtifactId.Create(Guid.NewGuid()), graph.Workspace.Id, ArtifactTitle.Create("Atomic artifact").ShouldSucceed(), ArtifactType.Markdown, graph.Artifact.CreatedAtUtc).ShouldSucceed();
        ArtifactRevision revision = artifact.CreateRevision(ArtifactRevisionId.Create(Guid.NewGuid()), ArtifactContent.Create("Atomic revision").ShouldSucceed(), graph.ArtifactRevision.CreatedAtUtc).ShouldSucceed();

        await using EspadaDbContext dbContext = Fixture.CreateDbContext();
        dbContext.AddRange(artifact, revision);
        dbContext.Entry(revision).Property(nameof(ArtifactRevision.ArtifactId)).CurrentValue = ArtifactId.Create(Guid.NewGuid());

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.ForeignKeyViolation);

        await using EspadaDbContext verificationContext = Fixture.CreateDbContext();
        Assert.False(await verificationContext.Artifacts.AnyAsync(value => value.Id == artifact.Id, TestContext.Current.CancellationToken));
        Assert.False(await verificationContext.ArtifactRevisions.AnyAsync(value => value.Id == revision.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ChunkBatchAndChunks_WhenChunkConstraintFails_ShouldPersistNothing()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using (EspadaDbContext setupContext = Fixture.CreateDbContext())
        {
            setupContext.AddRange(graph.Workspace, graph.Artifact, graph.ArtifactRevision);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        ChunkBatch batch = ChunkBatch.Request(ChunkBatchId.Create(Guid.NewGuid()), graph.Workspace.Id, graph.Artifact.Id, graph.ArtifactRevision.Id, ChunkingStrategyType.Recursive, ChunkingVersion.Create("recursive-v1").ShouldSucceed(), graph.ChunkBatch.RequestedAtUtc).ShouldSucceed();
        Chunk first = CreateChunk(batch, 1, "First chunk");
        Chunk duplicate = CreateChunk(batch, 1, "Duplicate chunk");

        await using EspadaDbContext dbContext = Fixture.CreateDbContext();
        dbContext.AddRange(batch, first, duplicate);

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);

        await using EspadaDbContext verificationContext = Fixture.CreateDbContext();
        Assert.False(await verificationContext.ChunkBatches.AnyAsync(value => value.Id == batch.Id, TestContext.Current.CancellationToken));
        Assert.False(await verificationContext.Chunks.AnyAsync(value => value.BatchId == batch.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EmbeddingMetadataAndVector_WhenMetadataConstraintFails_ShouldPersistNeither()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();

        using (IServiceScope setupScope = serviceProvider.CreateScope())
        {
            EspadaDbContext setupContext = setupScope.ServiceProvider.GetRequiredService<EspadaDbContext>();
            setupContext.AddRange(graph.Workspace, graph.Artifact, graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);
            await setupScope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(graph.ChunkEmbedding.Id, [0.1f, 0.2f, 0.3f], TestContext.Current.CancellationToken);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        ChunkEmbedding duplicate = ChunkEmbedding.Create(ChunkEmbeddingId.Create(Guid.NewGuid()), graph.Workspace.Id, graph.Chunk.Id, graph.Chunk.ContentHash, graph.ChunkEmbedding.Model, graph.ChunkEmbedding.Dimensions, graph.ChunkEmbedding.CreatedAtUtc).ShouldSucceed();

        using (IServiceScope failingScope = serviceProvider.CreateScope())
        {
            IServiceProvider services = failingScope.ServiceProvider;
            await services.GetRequiredService<IChunkEmbeddingRepository>().AddAsync(duplicate, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(duplicate.Id, [0.4f, 0.5f, 0.6f], TestContext.Current.CancellationToken);
            await Assert.ThrowsAsync<DbUpdateException>(() => services.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken));
        }

        using IServiceScope verificationScope = serviceProvider.CreateScope();
        Assert.Null(await verificationScope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>().GetByIdAsync(duplicate.Id, TestContext.Current.CancellationToken));
    }

    private static Chunk CreateChunk(ChunkBatch batch, int number, string content) =>
        Chunk.Create(ChunkId.Create(Guid.NewGuid()), batch.Id, batch.WorkspaceId, batch.ArtifactId, batch.ArtifactRevisionId, ChunkNumber.Create(number).ShouldSucceed(), ChunkContent.Create(content).ShouldSucceed(), null, batch.Strategy, batch.StrategyVersion, batch.RequestedAtUtc).ShouldSucceed();

    private static async Task AssertDatabaseViolationAsync(EspadaDbContext dbContext, string expectedSqlState)
    {
        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
        PostgresException postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(expectedSqlState, postgresException.SqlState);
    }
}