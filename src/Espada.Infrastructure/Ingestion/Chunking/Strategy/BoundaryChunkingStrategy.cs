using Espada.Application.Contracts.Ingestion;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal class BoundaryChunkingStrategy(string name, IReadOnlyList<string> separators) : IChunkingStrategy
{
    public string Name { get; } = name;

    protected virtual IReadOnlyList<string> ResolveSeparators(ImportOptions options) => separators;

    public Task<IReadOnlyList<ChunkSegment>> ChunkAsync(string content, ImportOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FixedSizeChunkingStrategy.Validate(content, options);
        
        IReadOnlyList<string> boundaries = ResolveSeparators(options);
        List<ChunkSegment> chunks = [];
        int start = 0;
        int number = 1;
        while (start < content.Length)
        {
            int hardEnd = Math.Min(content.Length, start + options.MaxCharacters);
            int end = hardEnd;
            if (hardEnd < content.Length)
            {
                foreach (string boundary in boundaries)
                {
                    int candidate = content.LastIndexOf(boundary, hardEnd - 1, hardEnd - start, StringComparison.Ordinal);
                    if (candidate <= start)
                    {
                        continue;
                    }

                    end = candidate + boundary.Length;
                    break;
                }
            }

            int segmentStart = start;
            int segmentEnd = end;
            while (segmentStart < segmentEnd && char.IsWhiteSpace(content[segmentStart]))
            {
                segmentStart++;
            }
            while (segmentEnd > segmentStart && char.IsWhiteSpace(content[segmentEnd - 1]))
            {
                segmentEnd--;
            }

            if (segmentEnd > segmentStart)
            {
                chunks.Add(new ChunkSegment(number++, content.Substring(segmentStart, segmentEnd - segmentStart), segmentStart, segmentEnd - segmentStart));
            }

            if (end == content.Length)
            {
                break;
            }

            start = Math.Max(start + 1, end - options.OverlapCharacters);
        }

        return Task.FromResult<IReadOnlyList<ChunkSegment>>(chunks);
    }
}