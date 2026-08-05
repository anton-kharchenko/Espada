using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    internal static class UnifiedSearchApplicationErrors
    {
        public static readonly DomainError QueryEmpty = new(
            "UnifiedSearch.Query.Empty",
            "Search query is required.");
        public static readonly DomainError LimitOutOfRange = new(
            "UnifiedSearch.Limit.OutOfRange",
            "Search limit must be between 1 and 50.");
        public static readonly DomainError InvalidEmbeddingModel = new(
            "UnifiedSearch.EmbeddingModel.Invalid",
            "The configured embedding model must use identifier@version format.");
    }
}