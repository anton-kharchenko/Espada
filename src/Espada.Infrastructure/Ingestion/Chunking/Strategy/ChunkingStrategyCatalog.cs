using Espada.Application.Contracts.Ingestion;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal sealed class ChunkingStrategyCatalog
{
    private readonly IReadOnlyDictionary<string, IChunkingStrategy> _strategies;

    public ChunkingStrategyCatalog(IEnumerable<IChunkingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(strategy => strategy.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<IChunkingStrategy> Strategies => [.. _strategies.Values];

    public IChunkingStrategy Get(string name) => _strategies.TryGetValue(name, out IChunkingStrategy? strategy) ? strategy : throw new InvalidOperationException($"Chunking strategy '{name}' is not registered.");
}