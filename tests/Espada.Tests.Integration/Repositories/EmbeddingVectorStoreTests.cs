using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Integration.Repositories
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class EmbeddingVectorStoreTests(PostgreSqlDatabaseFixture fixture)
        : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task AddAndGet_ShouldRoundTripVector()
        {
            PersistenceGraph graph = PersistenceGraphFactory.Create();
            float[] expected = [0.25f, -0.5f, 1.25f];

            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();

            using (IServiceScope writeScope = serviceProvider.CreateScope())
            {
                EspadaDbContext dbContext = writeScope.ServiceProvider.GetRequiredService<EspadaDbContext>();
                dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact,
                    graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);

                IEmbeddingVectorStore vectorStore =
                    writeScope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
                await vectorStore.UpsertAsync(graph.ChunkEmbedding.Id, expected, TestContext.Current.CancellationToken);
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            using IServiceScope readScope = serviceProvider.CreateScope();
            IEmbeddingVectorStore readStore = readScope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
            IReadOnlyList<float>? actual =
                await readStore.GetByIdAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Upsert_ShouldReplaceExistingVector()
        {
            PersistenceGraph graph = PersistenceGraphFactory.Create();
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
                dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact,
                    graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);
                IEmbeddingVectorStore store = scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
                await store.UpsertAsync(graph.ChunkEmbedding.Id, [1f, 0f, 0f], TestContext.Current.CancellationToken);
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                IEmbeddingVectorStore store = scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
                await store.UpsertAsync(graph.ChunkEmbedding.Id, [0f, 1f, 0f], TestContext.Current.CancellationToken);
                await scope.ServiceProvider.GetRequiredService<EspadaDbContext>()
                    .SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            using IServiceScope readScope = serviceProvider.CreateScope();
            IReadOnlyList<float>? actual = await readScope.ServiceProvider
                .GetRequiredService<IEmbeddingVectorStore>()
                .GetByIdAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken);

            Assert.Equal([0f, 1f, 0f], actual);
        }

        [Fact]
        public async Task SearchNearest_ShouldReturnMatchingWorkspaceModel()
        {
            PersistenceGraph graph = PersistenceGraphFactory.Create();
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
            await SeedGraphAsync(serviceProvider, graph, [1f, 0f, 0f]);

            using IServiceScope scope = serviceProvider.CreateScope();
            IEmbeddingVectorStore store = scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
            IReadOnlyList<EmbeddingVectorSearchHit> hits = await store.SearchNearestAsync(
                new EmbeddingVectorSearch(graph.Workspace.Id, graph.ChunkEmbedding.Model, [1f, 0f, 0f], 10, 0.99),
                TestContext.Current.CancellationToken);

            EmbeddingVectorSearchHit hit = Assert.Single(hits);
            Assert.Equal(graph.ChunkEmbedding.Id, hit.ChunkEmbeddingId);
            Assert.Equal(graph.Chunk.Id, hit.ChunkId);
            Assert.Equal(1d, hit.Similarity, 6);
        }

        [Fact]
        public async Task DeleteAndDeleteByWorkspace_ShouldRemoveVectors()
        {
            PersistenceGraph graph = PersistenceGraphFactory.Create();
            await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
            await SeedGraphAsync(serviceProvider, graph, [1f, 0f, 0f]);

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                IEmbeddingVectorStore store = scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
                await store.DeleteAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken);
            }

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                IEmbeddingVectorStore store = scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
                Assert.Null(await store.GetByIdAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken));
                await store.UpsertAsync(graph.ChunkEmbedding.Id, [1f, 0f, 0f], TestContext.Current.CancellationToken);
                await scope.ServiceProvider.GetRequiredService<EspadaDbContext>()
                    .SaveChangesAsync(TestContext.Current.CancellationToken);
                await store.DeleteByWorkspaceAsync(graph.Workspace.Id, TestContext.Current.CancellationToken);
            }

            using IServiceScope readScope = serviceProvider.CreateScope();
            Assert.Null(await readScope.ServiceProvider
                .GetRequiredService<IEmbeddingVectorStore>()
                .GetByIdAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken));
        }

        private static async Task SeedGraphAsync(ServiceProvider serviceProvider, PersistenceGraph graph,
            IReadOnlyList<float> vector)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
            dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact, graph.ArtifactRevision,
                graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);
            await scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(graph.ChunkEmbedding.Id,
                vector, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }
}