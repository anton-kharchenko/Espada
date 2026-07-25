using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Chunks;

public sealed class CreateChunkItemRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int Number { get; init; }

    public string Content { get; init; } = string.Empty;

    public int? SourceStart { get; init; }

    public int? SourceLength { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            yield return new ValidationResult("Chunk content cannot be empty.", [nameof(Content)]);
        }

        if (SourceStart.HasValue != SourceLength.HasValue)
        {
            yield return new ValidationResult("Source start and source length must be provided together.", [nameof(SourceStart), nameof(SourceLength)]);
        }

        if (SourceStart is < 0)
        {
            yield return new ValidationResult("Source start cannot be negative.", [nameof(SourceStart)]);
        }

        if (SourceLength is <= 0)
        {
            yield return new ValidationResult("Source length must be greater than zero.", [nameof(SourceLength)]);
        }
    }
}