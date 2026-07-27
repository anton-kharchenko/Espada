using Espada.Application.UseCases.Imports.Commands.RequestImport;

namespace Espada.Infrastructure.Ingestion.Chunking.Strategy;

internal sealed class CustomChunkingStrategy() : BoundaryChunkingStrategy("Custom", [])
{
    protected override IReadOnlyList<string> ResolveSeparators(ImportOptions options) =>
        options.Separators is { Count: > 0 }
            ? options.Separators.Where(separator => !string.IsNullOrEmpty(separator)).ToArray()
            : throw new ArgumentException("Custom chunking requires at least one non-empty separator.", nameof(options));
}