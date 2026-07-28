using Espada.Application.Models;
using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Application.Contracts.Ingestion
{
    public interface IChunkingStrategy
    {
        string Name { get; }

        Task<IReadOnlyList<ChunkSegment>> ChunkAsync(string content, ImportOptions options,
            CancellationToken cancellationToken = default);
    }
}