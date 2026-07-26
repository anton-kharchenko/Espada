using Espada.Application.Contracts.Persistence;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Integration.Repositories;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class EmbeddingVectorStoreTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
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
            dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact, graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);

            IEmbeddingVectorStore vectorStore = writeScope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
            await vectorStore.AddAsync(graph.ChunkEmbedding.Id, expected, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using IServiceScope readScope = serviceProvider.CreateScope();
        IEmbeddingVectorStore readStore = readScope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>();
        IReadOnlyList<float>? actual = await readStore.GetByIdAsync(graph.ChunkEmbedding.Id, TestContext.Current.CancellationToken);

        Assert.Equal(expected, actual);
    }
}