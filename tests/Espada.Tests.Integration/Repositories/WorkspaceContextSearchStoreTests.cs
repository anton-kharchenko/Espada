using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Repositories;
using Espada.Tests.Integration.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.Extensions.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task Search_WithHybridQuery_ShouldKeepFtsOnlyMemory()
    {
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        (Guid workspaceId, RememberMemoryResponse vectorMemory, RememberMemoryResponse ftsMemory) =
            await CreateMemoryPairAsync(serviceProvider, "hybridfallback");
        await AddEmbeddingAsync(
            serviceProvider,
            vectorMemory.ArtifactId,
            "current-model",
            "2");

        WorkspaceContextSearch search = CreateMemorySearch(
            workspaceId,
            "hybridfallback",
            "current-model",
            "2");
        using IServiceScope scope = serviceProvider.CreateScope();
        IWorkspaceContextSearchStore store =
            scope.ServiceProvider.GetRequiredService<IWorkspaceContextSearchStore>();

        IReadOnlyList<WorkspaceContextSearchHit> hits = await store.SearchAsync(
            search,
            TestContext.Current.CancellationToken);

        Assert.Contains(vectorMemory.ArtifactId, hits.Select(hit => hit.ArtifactId));
        WorkspaceContextSearchHit ftsHit = hits.Single(hit =>
            hit.ArtifactId == ftsMemory.ArtifactId);
        Assert.True(ftsHit.KeywordScore > 0d);
        Assert.Equal(0d, ftsHit.Similarity);
    }

    [Fact]
    public async Task Search_AfterEmbeddingModelChange_ShouldKeepPreviousModelMemory()
    {
        await using ServiceProvider serviceProvider = Fixture.CreateServiceProvider();
        (Guid workspaceId, RememberMemoryResponse previousModelMemory, _) =
            await CreateMemoryPairAsync(serviceProvider, "modelchangefallback");
        await AddEmbeddingAsync(
            serviceProvider,
            previousModelMemory.ArtifactId,
            "previous-model",
            "1");

        WorkspaceContextSearch search = CreateMemorySearch(
            workspaceId,
            "modelchangefallback",
            "current-model",
            "2");
        using IServiceScope scope = serviceProvider.CreateScope();
        IWorkspaceContextSearchStore store =
            scope.ServiceProvider.GetRequiredService<IWorkspaceContextSearchStore>();

        IReadOnlyList<WorkspaceContextSearchHit> hits = await store.SearchAsync(
            search,
            TestContext.Current.CancellationToken);

        WorkspaceContextSearchHit hit = hits.Single(candidate =>
            candidate.ArtifactId == previousModelMemory.ArtifactId);
        Assert.True(hit.KeywordScore > 0d);
        Assert.Equal(0d, hit.Similarity);
    }

    private static async Task<(
        Guid WorkspaceId,
        RememberMemoryResponse First,
        RememberMemoryResponse Second)> CreateMemoryPairAsync(
        ServiceProvider serviceProvider,
        string searchTerm)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        DomainResult<CreateWorkspaceResponse> workspaceResult = await mediator.Send(
            new CreateWorkspaceCommand(
                $"Search workspace {Guid.NewGuid():N}",
                WorkspaceType.Personal),
            cancellationToken);
        DomainResult<RememberMemoryResponse> firstResult = await mediator.Send(
            new RememberMemoryCommand(
                workspaceResult.Value.WorkspaceId,
                "First memory",
                $"{searchTerm} first memory",
                MemoryCategoryType.Fact.Id,
                0.8m,
                "integration-test"),
            cancellationToken);
        DomainResult<RememberMemoryResponse> secondResult = await mediator.Send(
            new RememberMemoryCommand(
                workspaceResult.Value.WorkspaceId,
                "Second memory",
                $"{searchTerm} second memory",
                MemoryCategoryType.Fact.Id,
                0.8m,
                "integration-test"),
            cancellationToken);

        return (workspaceResult.Value.WorkspaceId, firstResult.Value, secondResult.Value);
    }

    private static async Task AddEmbeddingAsync(
        ServiceProvider serviceProvider,
        Guid artifactId,
        string modelIdentifier,
        string modelVersion)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();
        ArtifactId domainArtifactId = ArtifactId.Create(artifactId);
        Chunk chunk = await dbContext.Chunks.SingleAsync(
            candidate => candidate.ArtifactId == domainArtifactId,
            TestContext.Current.CancellationToken);
        ChunkEmbedding embedding = ChunkEmbedding.Create(
            ChunkEmbeddingId.Create(Guid.NewGuid()),
            chunk.WorkspaceId,
            chunk.Id,
            chunk.ContentHash,
            EmbeddingModel.Create(modelIdentifier, modelVersion).Value,
            EmbeddingDimensions.Create(3).Value,
            chunk.CreatedAtUtc).Value;
        dbContext.ChunkEmbeddings.Add(embedding);
        await scope.ServiceProvider.GetRequiredService<IEmbeddingVectorStore>().UpsertAsync(
            embedding.Id,
            [1f, 0f, 0f],
            TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static WorkspaceContextSearch CreateMemorySearch(
        Guid workspaceId,
        string queryText,
        string modelIdentifier,
        string modelVersion)
    {
        return new WorkspaceContextSearch(
            workspaceId,
            queryText,
            [1f, 0f, 0f],
            modelIdentifier,
            modelVersion,
            10,
            [],
            [],
            [],
            [],
            [ArtifactKindType.Memory.Name],
            [],
            [],
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            true);
    }
}