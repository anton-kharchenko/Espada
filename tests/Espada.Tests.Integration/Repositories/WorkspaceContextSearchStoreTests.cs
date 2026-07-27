using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Repositories;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Integration.Repositories;

[Collection(PostgreSqlIntegrationCollection.Name)]
public sealed class WorkspaceContextSearchStoreTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
{
    [Fact]
    public async Task Search_ShouldReturnHybridScoresAndApplyPriorityFilter()
    {
        PersistenceGraph graph = PersistenceGraphFactory.Create();
        graph.Artifact.SetPriority(ContextPriority.Create(50).Value, graph.Chunk.CreatedAtUtc).ShouldSucceed();
        graph.Source.SetPriority(ContextPriority.Create(-25).Value, graph.Chunk.CreatedAtUtc).ShouldSucceed();

        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
            dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact, graph.ArtifactRevision, graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);
            await scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(graph.ChunkEmbedding.Id, [1f, 0f, 0f], TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        WorkspaceContextSearch search = new(
            graph.Workspace.Id.Value,
            "Integration chunk",
            [1f, 0f, 0f],
            graph.ChunkEmbedding.Model.Identifier,
            graph.ChunkEmbedding.Model.Version,
            10,
            [],
            [],
            [],
            [],
            [],
            null,
            0.99,
            null,
            null,
            graph.Chunk.CreatedAtUtc);

        using IServiceScope readScope = serviceProvider.CreateScope();
        IWorkspaceContextSearchStore store = readScope.ServiceProvider.GetRequiredService<IWorkspaceContextSearchStore>();
        WorkspaceContextSearchHit hit = Assert.Single(await store.SearchAsync(search, TestContext.Current.CancellationToken));

        Assert.Equal(graph.Chunk.Id.Value, hit.ChunkId);
        Assert.Equal(graph.Artifact.Id.Value, hit.ArtifactId);
        Assert.Equal(graph.ArtifactRevision.Id.Value, hit.RevisionId);
        Assert.Equal(graph.Chunk.Content.Value, hit.Content);
        Assert.Equal(1d, hit.Similarity, precision: 6);
        Assert.True(hit.KeywordScore > 0);
        Assert.Equal(1d, hit.RecencyScore, precision: 6);
        Assert.Equal(0.5d, hit.ArtifactPriorityScore, precision: 6);
        Assert.Equal(-0.25d, hit.SourcePriorityScore, precision: 6);
        Assert.InRange(hit.Score, 0d, 1d);

        IReadOnlyList<WorkspaceContextSearchHit> filtered = await store.SearchAsync(search with { MinimumArtifactPriority = 51 }, TestContext.Current.CancellationToken);
        Assert.Empty(filtered);

        IConfiguration vectorOnlyConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{WorkspaceContextSearchOptions.SectionName}:{nameof(WorkspaceContextSearchOptions.VectorWeight)}"] = "1",
                [$"{WorkspaceContextSearchOptions.SectionName}:{nameof(WorkspaceContextSearchOptions.KeywordWeight)}"] = "0",
                [$"{WorkspaceContextSearchOptions.SectionName}:{nameof(WorkspaceContextSearchOptions.RecencyWeight)}"] = "0",
                [$"{WorkspaceContextSearchOptions.SectionName}:{nameof(WorkspaceContextSearchOptions.ArtifactPriorityWeight)}"] = "0",
                [$"{WorkspaceContextSearchOptions.SectionName}:{nameof(WorkspaceContextSearchOptions.SourcePriorityWeight)}"] = "0"
            })
            .Build();

        await using ServiceProvider vectorOnlyProvider = Fixture.CreateServiceProvider(vectorOnlyConfiguration);
        using IServiceScope vectorOnlyScope = vectorOnlyProvider.CreateScope();
        IWorkspaceContextSearchStore vectorOnlyStore = vectorOnlyScope.ServiceProvider.GetRequiredService<IWorkspaceContextSearchStore>();
        WorkspaceContextSearchHit vectorOnlyHit = Assert.Single(
            await vectorOnlyStore.SearchAsync(search, TestContext.Current.CancellationToken));

        Assert.Equal(1d, vectorOnlyHit.Score, precision: 6);
    }
}