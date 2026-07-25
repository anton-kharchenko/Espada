using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.ChunkEmbeddings;

public sealed class CreateChunkEmbeddingRequest : IValidatableObject
{
    public string ModelIdentifier { get; init; } = string.Empty;

    public string ModelVersion { get; init; } = string.Empty;

    public IReadOnlyList<float> Vector { get; init; } = Array.Empty<float>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ModelIdentifier))
        {
            yield return new ValidationResult("Embedding model identifier cannot be empty.", new[] { nameof(ModelIdentifier) });
        }

        if (string.IsNullOrWhiteSpace(ModelVersion))
        {
            yield return new ValidationResult("Embedding model version cannot be empty.", new[] { nameof(ModelVersion) });
        }

        if (Vector.Count == 0)
        {
            yield return new ValidationResult("Embedding vector cannot be empty.", new[] { nameof(Vector) });
        }
    }
}