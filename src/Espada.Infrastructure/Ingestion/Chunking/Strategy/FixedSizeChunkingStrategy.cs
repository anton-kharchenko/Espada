using Espada.Application.Contracts.Ingestion;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal sealed class FixedSizeChunkingStrategy : IChunkingStrategy
{
    public string Name => "FixedSize";

    public Task<IReadOnlyList<ChunkSegment>> ChunkAsync(string content, ImportOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(content, options);
        List<ChunkSegment> chunks = [];
        int step = options.MaxCharacters - options.OverlapCharacters;
        
        for (int start = 0, number = 1; start < content.Length; start += step, number++)
        {
            int length = Math.Min(options.MaxCharacters, content.Length - start);
            chunks.Add(new ChunkSegment(number, content.Substring(start, length), start, length));
            if (start + length == content.Length)
            {
                break;
            }
        }

        return Task.FromResult<IReadOnlyList<ChunkSegment>>(chunks);
    }

    internal static void Validate(string content, ImportOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentNullException.ThrowIfNull(options);
        if (options.MaxCharacters <= 0 || options.OverlapCharacters < 0 || options.OverlapCharacters >= options.MaxCharacters)
        {
            throw new ArgumentOutOfRangeException(nameof(options));
        }
    }
}