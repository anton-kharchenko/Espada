using Espada.Application.Models;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    public sealed record ImportOptions(
        string? EmbeddingModel = null,
        string ChunkingStrategy = "Recursive",
        int MaxCharacters = 2000,
        int OverlapCharacters = 200,
        double SemanticThreshold = 0.75,
        IReadOnlyList<string>? Separators = null,
        string? CodeLanguage = null,
        RepositoryFileImportOptions? RepositoryFile = null);
}