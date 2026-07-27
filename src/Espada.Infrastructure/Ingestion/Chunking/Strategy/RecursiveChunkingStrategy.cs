namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal sealed class RecursiveChunkingStrategy() : BoundaryChunkingStrategy("Recursive", ["\n\n", "\n", ". ", " "]);