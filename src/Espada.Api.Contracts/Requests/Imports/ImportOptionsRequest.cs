using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Imports
{
    public sealed class ImportOptionsRequest : IValidatableObject
    {
        private static readonly HashSet<string> SupportedStrategies =
            new(["FixedSize", "Recursive", "Markdown", "Semantic", "Code", "Custom"], StringComparer.OrdinalIgnoreCase);

        public string? EmbeddingModel { get; init; }

        public string ChunkingStrategy { get; init; } = "Recursive";

        public int MaxCharacters { get; init; } = 2000;

        public int OverlapCharacters { get; init; } = 200;

        public double SemanticThreshold { get; init; } = 0.75;

        public IReadOnlyList<string>? Separators { get; init; }

        public string? CodeLanguage { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!SupportedStrategies.Contains(ChunkingStrategy))
            {
                yield return new ValidationResult($"Unsupported chunking strategy '{ChunkingStrategy}'.",
                    [nameof(ChunkingStrategy)]);
            }

            if (MaxCharacters <= 0)
            {
                yield return new ValidationResult("MaxCharacters must be positive.", [nameof(MaxCharacters)]);
            }

            if (OverlapCharacters < 0 || OverlapCharacters >= MaxCharacters)
            {
                yield return new ValidationResult("OverlapCharacters must be non-negative and less than MaxCharacters.",
                    [nameof(OverlapCharacters)]);
            }

            if (SemanticThreshold is < 0 or > 1)
            {
                yield return new ValidationResult("SemanticThreshold must be between 0 and 1.",
                    [nameof(SemanticThreshold)]);
            }
        }
    }
}