using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Espada.Tests.Integration.Database;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class AggregateRoundTripTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
{
    [Fact]
    public async Task Workspace_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Workspace actual = Assert.IsType<Workspace>(await dbContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.Workspace.Id, actual.Id);
        Assert.Equal(graph.Workspace.Name, actual.Name);
        Assert.Equal(graph.Workspace.Type, actual.Type);
        Assert.Equal(graph.Workspace.Status, actual.Status);
        Assert.Equal(graph.Workspace.CreatedAtUtc, actual.CreatedAtUtc);
        Assert.Equal(graph.Workspace.ArchivedAtUtc, actual.ArchivedAtUtc);
        Assert.Equal(graph.Workspace.Version, actual.Version);
    }

    [Fact]
    public async Task Source_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Source actual = Assert.IsType<Source>(await dbContext.Sources.FindAsync([graph.Source.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.Source.Id, actual.Id);
        Assert.Equal(graph.Source.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(graph.Source.Name, actual.Name);
        Assert.Equal(graph.Source.Type, actual.Type);
        Assert.Equal(graph.Source.Locator, actual.Locator);
        Assert.Equal(graph.Source.Status, actual.Status);
        Assert.Equal(graph.Source.CreatedAtUtc, actual.CreatedAtUtc);
        Assert.Equal(graph.Source.UpdatedAtUtc, actual.UpdatedAtUtc);
        Assert.Equal(graph.Source.ArchivedAtUtc, actual.ArchivedAtUtc);
        Assert.Equal(graph.Source.Version, actual.Version);
    }

    [Fact]
    public async Task ImportJob_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        ImportJob actual = Assert.IsType<ImportJob>(await dbContext.ImportJobs.FindAsync([graph.ImportJob.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.ImportJob.Id, actual.Id);
        Assert.Equal(graph.ImportJob.SourceId, actual.SourceId);
        Assert.Equal(graph.ImportJob.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(graph.ImportJob.Status, actual.Status);
        Assert.Equal(graph.ImportJob.RequestedAtUtc, actual.RequestedAtUtc);
        Assert.Equal(graph.ImportJob.StartedAtUtc, actual.StartedAtUtc);
        Assert.Equal(graph.ImportJob.CompletedAtUtc, actual.CompletedAtUtc);
        Assert.Equal(graph.ImportJob.ArtifactId, actual.ArtifactId);
        Assert.Equal(graph.ImportJob.ArtifactRevisionId, actual.ArtifactRevisionId);
        Assert.Equal(graph.ImportJob.Failure, actual.Failure);
        Assert.Equal(graph.ImportJob.Version, actual.Version);
    }

    [Fact]
    public async Task Artifact_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Artifact actual = Assert.IsType<Artifact>(await dbContext.Artifacts.FindAsync([graph.Artifact.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.Artifact.Id, actual.Id);
        Assert.Equal(graph.Artifact.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(graph.Artifact.Title, actual.Title);
        Assert.Equal(graph.Artifact.Type, actual.Type);
        Assert.Equal(graph.Artifact.Status, actual.Status);
        Assert.Equal(graph.Artifact.CreatedAtUtc, actual.CreatedAtUtc);
        Assert.Equal(graph.Artifact.CurrentRevisionId, actual.CurrentRevisionId);
        Assert.Equal(graph.Artifact.CurrentRevisionNumber, actual.CurrentRevisionNumber);
        Assert.Equal(graph.Artifact.UpdatedAtUtc, actual.UpdatedAtUtc);
        Assert.Equal(graph.Artifact.ArchivedAtUtc, actual.ArchivedAtUtc);
        Assert.Equal(graph.Artifact.RevisionCount, actual.RevisionCount);
        Assert.Equal(graph.Artifact.Version, actual.Version);
    }

    [Fact]
    public async Task ArtifactRevision_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        ArtifactRevision actual = Assert.IsType<ArtifactRevision>(await dbContext.ArtifactRevisions.FindAsync([graph.ArtifactRevision.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.ArtifactRevision.Id, actual.Id);
        Assert.Equal(graph.ArtifactRevision.ArtifactId, actual.ArtifactId);
        Assert.Equal(graph.ArtifactRevision.Number, actual.Number);
        Assert.Equal(graph.ArtifactRevision.Content, actual.Content);
        Assert.Equal(graph.ArtifactRevision.ContentHash, actual.ContentHash);
        Assert.Equal(graph.ArtifactRevision.SizeInBytes, actual.SizeInBytes);
        Assert.Equal(graph.ArtifactRevision.CreatedAtUtc, actual.CreatedAtUtc);
    }

    [Fact]
    public async Task ChunkBatch_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        ChunkBatch actual = Assert.IsType<ChunkBatch>(await dbContext.ChunkBatches.FindAsync([graph.ChunkBatch.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.ChunkBatch.Id, actual.Id);
        Assert.Equal(graph.ChunkBatch.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(graph.ChunkBatch.ArtifactId, actual.ArtifactId);
        Assert.Equal(graph.ChunkBatch.ArtifactRevisionId, actual.ArtifactRevisionId);
        Assert.Equal(graph.ChunkBatch.Strategy, actual.Strategy);
        Assert.Equal(graph.ChunkBatch.StrategyVersion, actual.StrategyVersion);
        Assert.Equal(graph.ChunkBatch.Status, actual.Status);
        Assert.Equal(graph.ChunkBatch.RequestedAtUtc, actual.RequestedAtUtc);
        Assert.Equal(graph.ChunkBatch.StartedAtUtc, actual.StartedAtUtc);
        Assert.Equal(graph.ChunkBatch.CompletedAtUtc, actual.CompletedAtUtc);
        Assert.Equal(graph.ChunkBatch.ChunkCount, actual.ChunkCount);
        Assert.Equal(graph.ChunkBatch.FailureReason, actual.FailureReason);
        Assert.Equal(graph.ChunkBatch.Version, actual.Version);
    }

    [Fact]
    public async Task Chunk_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Chunk actual = Assert.IsType<Chunk>(await dbContext.Chunks.FindAsync([graph.Chunk.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.Chunk.Id, actual.Id);
        Assert.Equal(graph.Chunk.BatchId, actual.BatchId);
        Assert.Equal(graph.Chunk.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(graph.Chunk.ArtifactId, actual.ArtifactId);
        Assert.Equal(graph.Chunk.ArtifactRevisionId, actual.ArtifactRevisionId);
        Assert.Equal(graph.Chunk.Number, actual.Number);
        Assert.Equal(graph.Chunk.Content, actual.Content);
        Assert.Equal(graph.Chunk.SourceSpan, actual.SourceSpan);
        Assert.Equal(graph.Chunk.Strategy, actual.Strategy);
        Assert.Equal(graph.Chunk.StrategyVersion, actual.StrategyVersion);
        Assert.Equal(graph.Chunk.ContentHash, actual.ContentHash);
        Assert.Equal(graph.Chunk.SizeInBytes, actual.SizeInBytes);
        Assert.Equal(graph.Chunk.CharacterCount, actual.CharacterCount);
        Assert.Equal(graph.Chunk.CreatedAtUtc, actual.CreatedAtUtc);
    }

    [Fact]
    public async Task ChunkEmbedding_ShouldRoundTripAllProperties()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        ChunkEmbedding actual = Assert.IsType<ChunkEmbedding>(await dbContext.ChunkEmbeddings.FindAsync([graph.ChunkEmbedding.Id], TestContext.Current.CancellationToken));

        Assert.Equal(graph.ChunkEmbedding.Id, actual.Id);
        Assert.Equal(graph.ChunkEmbedding.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(graph.ChunkEmbedding.ChunkId, actual.ChunkId);
        Assert.Equal(graph.ChunkEmbedding.ChunkContentHash, actual.ChunkContentHash);
        Assert.Equal(graph.ChunkEmbedding.Model, actual.Model);
        Assert.Equal(graph.ChunkEmbedding.Dimensions, actual.Dimensions);
        Assert.Equal(graph.ChunkEmbedding.CreatedAtUtc, actual.CreatedAtUtc);
    }

    [Fact]
    public async Task ForeignKeyConstraint_ShouldRejectMissingWorkspace()
    {
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Source source = Source.Create(SourceId.Create(Guid.NewGuid()), WorkspaceId.Create(Guid.NewGuid()), SourceName.Create("Orphan source").ShouldSucceed(), SourceType.WebPage, SourceLocator.Create($"https://example.com/{Guid.NewGuid():N}").ShouldSucceed(), new DateTimeOffset(2026, 7, 26, 5, 0, 0, TimeSpan.Zero)).ShouldSucceed();

        dbContext.Sources.Add(source);

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task SourceWorkspaceLocatorUniqueConstraint_ShouldRejectDuplicate()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Source duplicate = Source.Create(SourceId.Create(Guid.NewGuid()), graph.Source.WorkspaceId, SourceName.Create("Duplicate source").ShouldSucceed(), graph.Source.Type, graph.Source.Locator, graph.Source.CreatedAtUtc).ShouldSucceed();

        dbContext.Sources.Add(duplicate);

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task ArtifactRevisionArtifactNumberUniqueConstraint_ShouldRejectDuplicate()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Artifact revisionOwner = Artifact.Create(graph.Artifact.Id, graph.Artifact.WorkspaceId, graph.Artifact.Title, graph.Artifact.Type, graph.Artifact.CreatedAtUtc).ShouldSucceed();

        ArtifactRevision duplicate = revisionOwner.CreateRevision(ArtifactRevisionId.Create(Guid.NewGuid()), graph.ArtifactRevision.Content, graph.ArtifactRevision.CreatedAtUtc).ShouldSucceed();

        dbContext.ArtifactRevisions.Add(duplicate);

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task ChunkBatchIdNumberUniqueConstraint_ShouldRejectDuplicate()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        Chunk duplicate = Chunk.Create(ChunkId.Create(Guid.NewGuid()), graph.Chunk.BatchId, graph.Chunk.WorkspaceId, graph.Chunk.ArtifactId, graph.Chunk.ArtifactRevisionId, graph.Chunk.Number, graph.Chunk.Content, graph.Chunk.SourceSpan, graph.Chunk.Strategy, graph.Chunk.StrategyVersion, graph.Chunk.CreatedAtUtc).ShouldSucceed();

        dbContext.Chunks.Add(duplicate);

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task ChunkEmbeddingChunkModelUniqueConstraint_ShouldRejectDuplicate()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        ChunkEmbedding duplicate = ChunkEmbedding.Create(ChunkEmbeddingId.Create(Guid.NewGuid()), graph.ChunkEmbedding.WorkspaceId, graph.ChunkEmbedding.ChunkId, graph.ChunkEmbedding.ChunkContentHash, graph.ChunkEmbedding.Model, graph.ChunkEmbedding.Dimensions, graph.ChunkEmbedding.CreatedAtUtc).ShouldSucceed();

        dbContext.ChunkEmbeddings.Add(duplicate);

        await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task ConcurrentWorkspaceUpdates_ShouldThrowConcurrencyException()
    {
        PersistenceGraph graph = await PersistGraphAsync();
        await using EspadaDbContext firstContext = Fixture.CreateDbContext();
        await using EspadaDbContext secondContext = Fixture.CreateDbContext();

        Workspace first = Assert.IsType<Workspace>(await firstContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));
        Workspace second = Assert.IsType<Workspace>(await secondContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));
        uint originalVersion = first.Version;

        DateTimeOffset firstArchiveTime = new(2026, 7, 26, 6, 0, 0, TimeSpan.Zero);
        first.Archive(firstArchiveTime).ShouldSucceed();
        second.Archive(firstArchiveTime.AddMinutes(1)).ShouldSucceed();

        await firstContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.NotEqual(originalVersion, first.Version);

        await using EspadaDbContext verificationContext = Fixture.CreateDbContext();
        Workspace persisted = Assert.IsType<Workspace>(await verificationContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));

        Assert.Equal(first.Version, persisted.Version);
        Assert.Equal(firstArchiveTime, persisted.ArchivedAtUtc);
    }

    private async Task<PersistenceGraph> PersistGraphAsync()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using EspadaDbContext dbContext = Fixture.CreateDbContext();

        dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact, graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        return graph;
    }

    private static async Task AssertDatabaseViolationAsync(EspadaDbContext dbContext, string expectedSqlState)
    {
        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
        PostgresException postgresException = Assert.IsType<PostgresException>(exception.InnerException);

        Assert.Equal(expectedSqlState, postgresException.SqlState);
    }
}