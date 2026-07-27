using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Enums;
using Espada.Infrastructure.Models;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal sealed class SemanticChunkingStrategy(IBatchEmbeddingGeneratorService embeddingGenerator) : IChunkingStrategy
{
    public string Name => ChunkingStrategyType.Semantic.Name;

    public async Task<IReadOnlyList<ChunkSegment>> ChunkAsync(string content, ImportOptions options, CancellationToken cancellationToken = default)
    {
        FixedSizeChunkingStrategy.Validate(content, options);
        (string identifier, string version) = ParseModel(options.EmbeddingModel);
        IReadOnlyList<Sentence> sentences = SplitSentences(content);
        IReadOnlyList<GeneratedEmbedding> embeddings = await embeddingGenerator.GenerateBatchAsync(identifier, version, sentences.Select(sentence => sentence.Content).ToArray(), cancellationToken);
        if (embeddings.Count != sentences.Count)
        {
            throw new InvalidOperationException("Embedding provider returned a different number of vectors than inputs.");
        }

        List<ChunkSegment> chunks = [];
        int groupStart = 0;
        for (int index = 1; index <= sentences.Count; index++)
        {
            bool isEnd = index == sentences.Count;
            bool exceedsLength = !isEnd && sentences[index].End - sentences[groupStart].Start > options.MaxCharacters;
            bool similarityBreak = !isEnd
                && Cosine(embeddings[index - 1].Vector, embeddings[index].Vector)
                < options.SemanticThreshold;
            if (!isEnd && !exceedsLength && !similarityBreak)
            {
                continue;
            }

            int end = sentences[index - 1].End;
            int start = sentences[groupStart].Start;
            while (start < end && char.IsWhiteSpace(content[start]))
            {
                start++;
            }
            while (end > start && char.IsWhiteSpace(content[end - 1]))
            {
                end--;
            }
            chunks.Add(new ChunkSegment(chunks.Count + 1, content[start..end], start, end - start)); groupStart = index;
        }

        return chunks;
    }

    private static (string Identifier, string Version) ParseModel(string? model)
    {
        string[] parts = model?.Split('@', 2, StringSplitOptions.TrimEntries) ?? [];

        return parts.Length == 2 && parts.All(part => part.Length > 0) ? (parts[0], parts[1]) : throw new ArgumentException("Embedding model must use 'identifier@version' format.", nameof(model));
    }

    private static IReadOnlyList<Sentence> SplitSentences(string content)
    {
        List<Sentence> sentences = [];
        int start = 0;
        for (int index = 0; index < content.Length; index++)
        {
            if (content[index] is not ('.' or '!' or '?'))
            {
                continue;
            }

            int end = index + 1;
            if (end < content.Length && !char.IsWhiteSpace(content[end]))
            {
                continue;
            }

            while (start < end && char.IsWhiteSpace(content[start]))
            {
                start++;
            }
            sentences.Add(new Sentence(content[start..end], start, end));
            start = end;
        }

        if (start < content.Length)
        {
            int remainingStart = start;
            while (remainingStart < content.Length && char.IsWhiteSpace(content[remainingStart]))
            {
                remainingStart++;
            }
            if (remainingStart < content.Length)
            {
                sentences.Add(new Sentence(content[remainingStart..], remainingStart, content.Length));
            }
        }

        return sentences.Count > 0 ? sentences : [new Sentence(content, 0, content.Length)];
    }

    private static double Cosine(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        if (left.Count != right.Count || left.Count == 0)
        {
            throw new InvalidOperationException("Embedding vectors must have equal non-zero dimensions.");
        }

        double dot = 0;
        double leftMagnitude = 0;
        double rightMagnitude = 0;
        for (int index = 0; index < left.Count; index++)
        {
            dot += left[index] * right[index];
            leftMagnitude += left[index] * left[index];
            rightMagnitude += right[index] * right[index];
        }

        const double magnitudeEpsilon = 1e-12;
        if (leftMagnitude <= magnitudeEpsilon || rightMagnitude <= magnitudeEpsilon)
        {
            return 0;
        }

        return dot / (Math.Sqrt(leftMagnitude) * Math.Sqrt(rightMagnitude));
    }
}