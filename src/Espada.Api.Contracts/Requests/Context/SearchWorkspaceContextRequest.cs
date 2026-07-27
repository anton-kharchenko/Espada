using Espada.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Context;

public sealed class SearchWorkspaceContextRequest : IValidatableObject
{
    [Required]
    public string QueryText { get; init; } = string.Empty;

    [Required]
    public IReadOnlyList<float> QueryVector { get; init; } = [];

    [Required, StringLength(EmbeddingModel.IdentifierMaxLength)]
    public string ModelIdentifier { get; init; } = string.Empty;

    [Required, StringLength(EmbeddingModel.VersionMaxLength)]
    public string ModelVersion { get; init; } = string.Empty;

    [Range(1, 100)]
    public int TopK { get; init; } = 10;

    public IReadOnlyCollection<Guid> ArtifactIds { get; init; } = [];

    public IReadOnlyCollection<Guid> RevisionIds { get; init; } = [];

    public IReadOnlyCollection<Guid> SourceIds { get; init; } = [];

    public IReadOnlyCollection<int> ArtifactTypeIds { get; init; } = [];

    public IReadOnlyCollection<int> SourceTypeIds { get; init; } = [];

    public DateTimeOffset? CreatedAfterUtc { get; init; }

    [Range(-1d, 1d)]
    public double? MinimumSimilarity { get; init; }

    [Range(ContextPriority.Minimum, ContextPriority.Maximum)]
    public int? MinimumArtifactPriority { get; init; }

    [Range(ContextPriority.Minimum, ContextPriority.Maximum)]
    public int? MinimumSourcePriority { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (QueryVector.Count == 0)
        {
            yield return new ValidationResult("Query vector cannot be empty.", [nameof(QueryVector)]);
        }
        else if (QueryVector.Any(value => !float.IsFinite(value)))
        {
            yield return new ValidationResult("Query vector values must be finite.", [nameof(QueryVector)]);
        }
    }
}