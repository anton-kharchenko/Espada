using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Integration.Repositories;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class RepositoryTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
{
    [Fact]
    public async Task AddMethods_ShouldPersistAllRecords()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        float[] vector = [0.25f, -0.5f, 1.25f];

        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            await services.GetRequiredService<IWorkspaceRepository>().AddAsync(graph.Workspace, TestContext.Current.CancellationToken);
            await services.GetRequiredService<ISourceRepository>().AddAsync(graph.Source, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IImportJobRepository>().AddAsync(graph.ImportJob, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IArtifactRepository>().AddAsync(graph.Artifact, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IArtifactRevisionRepository>().AddAsync(graph.ArtifactRevision, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IChunkBatchRepository>().AddAsync(graph.ChunkBatch, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IChunkRepository>().AddRangeAsync([graph.Chunk], TestContext.Current.CancellationToken);
            await services.GetRequiredService<IChunkEmbeddingRepository>().AddAsync(graph.ChunkEmbedding, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(graph.ChunkEmbedding.Id, vector, TestContext.Current.CancellationToken);
            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using EspadaDbContext verificationContext = Fixture.CreateDbContext();
        Assert.Equal(1, await verificationContext.Workspaces.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.Sources.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.ImportJobs.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.Artifacts.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.ArtifactRevisions.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.ChunkBatches.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.Chunks.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await verificationContext.ChunkEmbeddings.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CommandQueries_ShouldReturnTrackedAggregates()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        await SeedGraphAsync(serviceProvider, graph);

        using IServiceScope scope = serviceProvider.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        EspadaDbContext dbContext = services.GetRequiredService<EspadaDbContext>();

        Workspace workspace = Assert.IsType<Workspace>(await services.GetRequiredService<IWorkspaceRepository>().GetByIdAsync(graph.Workspace.Id, TestContext.Current.CancellationToken));
        Source source = Assert.IsType<Source>(await services.GetRequiredService<ISourceRepository>().GetByIdAsync(graph.Source.Id, TestContext.Current.CancellationToken));
        ImportJob importJob = Assert.IsType<ImportJob>(await services.GetRequiredService<IImportJobRepository>().GetByIdAsync(graph.ImportJob.Id, TestContext.Current.CancellationToken));
        Artifact artifact = Assert.IsType<Artifact>(await services.GetRequiredService<IArtifactRepository>().GetByIdAsync(graph.Artifact.Id, TestContext.Current.CancellationToken));
        ChunkBatch chunkBatch = Assert.IsType<ChunkBatch>(await services.GetRequiredService<IChunkBatchRepository>().GetByIdAsync(graph.ChunkBatch.Id, TestContext.Current.CancellationToken));

        Assert.Equal(EntityState.Unchanged, dbContext.Entry(workspace).State);
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(source).State);
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(importJob).State);
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(artifact).State);
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(chunkBatch).State);
    }

    [Fact]
    public async Task ReadQueries_ShouldNotTrackAggregates()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        await SeedGraphAsync(serviceProvider, graph);

        using IServiceScope scope = serviceProvider.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        EspadaDbContext dbContext = services.GetRequiredService<EspadaDbContext>();

        Assert.NotNull(await services.GetRequiredService<IArtifactRevisionRepository>().GetByIdAsync(graph.ArtifactRevision.Id, TestContext.Current.CancellationToken));
        Assert.NotNull(await services.GetRequiredService<IChunkRepository>().GetByIdAsync(graph.Chunk.Id, TestContext.Current.CancellationToken));
        Assert.NotNull(await services.GetRequiredService<IChunkEmbeddingRepository>().GetByChunkIdAsync(graph.Chunk.Id, graph.ChunkEmbedding.Model, TestContext.Current.CancellationToken));
        Assert.Single(await services.GetRequiredService<IArtifactRepository>().ListByWorkspaceIdAsync(graph.Workspace.Id, TestContext.Current.CancellationToken));
        Assert.Single(await services.GetRequiredService<IArtifactRevisionRepository>().ListByArtifactIdAsync(graph.Artifact.Id, TestContext.Current.CancellationToken));
        Assert.Single(await services.GetRequiredService<IChunkRepository>().ListByArtifactRevisionIdAsync(graph.ArtifactRevision.Id, TestContext.Current.CancellationToken));
        Assert.NotNull(await services.GetRequiredService<IEmbeddingVectorStore>().GetByIdAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken));

        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(RepositoryKinds))]
    public async Task Queries_WithCanceledToken_ShouldCancel(RepositoryKind repository)

    {
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        async Task QueryAsync()
        {
            IServiceProvider services = scope.ServiceProvider;
            switch (repository)
            {
                case RepositoryKind.Workspace:
                    await services.GetRequiredService<IWorkspaceRepository>().GetByIdAsync(WorkspaceId.New(), cancellation.Token);
                    break;
                case RepositoryKind.Source:
                    await services.GetRequiredService<ISourceRepository>().GetByIdAsync(SourceId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                case RepositoryKind.Import:
                    await services.GetRequiredService<IImportJobRepository>().GetByIdAsync(ImportJobId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                case RepositoryKind.Artifact:
                    await services.GetRequiredService<IArtifactRepository>().GetByIdAsync(ArtifactId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                case RepositoryKind.Revision:
                    await services.GetRequiredService<IArtifactRevisionRepository>().GetByIdAsync(ArtifactRevisionId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                case RepositoryKind.Batch:
                    await services.GetRequiredService<IChunkBatchRepository>().GetByIdAsync(ChunkBatchId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                case RepositoryKind.Chunk:
                    await services.GetRequiredService<IChunkRepository>().GetByIdAsync(ChunkId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                case RepositoryKind.Embedding:
                    await services.GetRequiredService<IChunkEmbeddingRepository>().GetByChunkIdAsync(ChunkId.Create(Guid.NewGuid()), EmbeddingModel.Create("test-embedding-model", "1").ShouldSucceed(), cancellation.Token);
                    break;
                case RepositoryKind.Vector:
                    await services.GetRequiredService<IEmbeddingVectorStore>().GetByIdAsync(ChunkEmbeddingId.Create(Guid.NewGuid()), cancellation.Token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(repository), repository, null);
            }
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(QueryAsync);
    }

    public static TheoryData<RepositoryKind> RepositoryKinds => new()
    {
        RepositoryKind.Workspace,
        RepositoryKind.Source,
        RepositoryKind.Import,
        RepositoryKind.Artifact,
        RepositoryKind.Revision,
        RepositoryKind.Batch,
        RepositoryKind.Chunk,
        RepositoryKind.Embedding,
        RepositoryKind.Vector
    };

    public enum RepositoryKind
    {
        Workspace,
        Source,
        Import,
        Artifact,
        Revision,
        Batch,
        Chunk,
        Embedding,
        Vector
    }

    private static async Task SeedGraphAsync(ServiceProvider serviceProvider, PersistenceGraph graph)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
        dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact, graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);
        await scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(graph.ChunkEmbedding.Id, [0.25f, -0.5f, 1.25f], TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}