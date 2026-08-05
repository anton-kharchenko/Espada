namespace Espada.Infrastructure.Ingestion.Chunking.Strategy
{
    internal sealed class MarkdownChunkingStrategy()
        : BoundaryChunkingStrategy("Markdown", ["\n# ", "\n## ", "\n### ", "\n\n", "\n", ". ", " "]);
}