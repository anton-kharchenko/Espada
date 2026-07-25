using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Imports;

public sealed class CompleteImportRequest : IValidatableObject
{
    public Guid ArtifactId { get; init; }

    public Guid ArtifactRevisionId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ArtifactId == Guid.Empty)
        {
            yield return new ValidationResult("Artifact ID cannot be empty.", new[] { nameof(ArtifactId) });
        }

        if (ArtifactRevisionId == Guid.Empty)
        {
            yield return new ValidationResult("Artifact revision ID cannot be empty.", new[] { nameof(ArtifactRevisionId) });
        }
    }
}