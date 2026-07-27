using Espada.Application.Contracts.Ingestion;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Infrastructure.Ingestion.Chunking.Strategy;
using Espada.Tests.Infrastructure.Ingestion.Fakes;

namespace Espada.Tests.Infrastructure.Ingestion;

public sealed class ChunkingStrategyTests
{
    [Fact]
    public async Task FixedSize_ShouldCreateDeterministicOverlappingSpans()
    {
        IChunkingStrategy strategy = new FixedSizeChunkingStrategy();

        IReadOnlyList<ChunkSegment> chunks = await strategy.ChunkAsync(
            "abcdefghij",
            new ImportOptions(
                MaxCharacters: 4,
                OverlapCharacters: 1),
            TestContext.Current.CancellationToken);

        Assert.Collection(
            chunks,
            chunk => Assert.Equal((0, 4, "abcd"), (chunk.Start, chunk.Length, chunk.Content)),
            chunk => Assert.Equal((3, 4, "defg"), (chunk.Start, chunk.Length, chunk.Content)),
            chunk => Assert.Equal((6, 4, "ghij"), (chunk.Start, chunk.Length, chunk.Content)));
    }

    [Fact]
    public Task Catalog_ShouldExposeAllSixStrategies()
    {
        try
        {
            ChunkingStrategyCatalog catalog = new(
            [
                new FixedSizeChunkingStrategy(),
                new RecursiveChunkingStrategy(),
                new MarkdownChunkingStrategy(),
                new CodeChunkingStrategy(),
            new SemanticChunkingStrategy(new TestBatchEmbeddingGeneratorService()),
                new CustomChunkingStrategy()
            ]);

            string[] names = catalog.Strategies.Select(strategy => strategy.Name).Order().ToArray();

            Assert.Equal(
                ["Code", "Custom", "FixedSize", "Markdown", "Recursive", "Semantic"],
                names);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    [Fact]
    public async Task Semantic_ShouldSplitWhenAdjacentSentenceSimilarityFallsBelowThreshold()
    {
        IChunkingStrategy strategy = new SemanticChunkingStrategy(
            new TestBatchEmbeddingGeneratorService());

        IReadOnlyList<ChunkSegment> chunks = await strategy.ChunkAsync(
            "Alpha. Similar. Different.",
            new ImportOptions(
                EmbeddingModel: "test@1",
                ChunkingStrategy: "Semantic",
                MaxCharacters: 2000,
                OverlapCharacters: 0,
                SemanticThreshold: 0.75),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, chunks.Count);
        Assert.Equal("Alpha. Similar.", chunks[0].Content);
        Assert.Equal("Different.", chunks[1].Content);
    }
}