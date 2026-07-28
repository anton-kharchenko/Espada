using Espada.Application.Contracts.Ingestion;
using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class TestChunkingStrategy : IChunkingStrategy
    {
        public string Name => "Recursive";

        public Task<IReadOnlyList<ChunkSegment>> ChunkAsync(
            string content,
            ImportOptions options,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ChunkSegment> segments =
            [
                new(1, content, 0, content.Length)
            ];
            return Task.FromResult(segments);
        }
    }
}