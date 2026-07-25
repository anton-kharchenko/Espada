using System.ComponentModel.DataAnnotations;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;

namespace Espada.Api.Contracts.Requests.Artifacts;

public sealed class CreateArtifactRequest : IValidatableObject
{
    [Required]
    public string Title { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TypeId { get; init; }

    [Required]
    public string Content { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Title cannot be empty.", new[] { nameof(Title) });
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            yield return new ValidationResult("Content cannot be empty.", new[] { nameof(Content) });
        }

        bool supported = Enumeration.GetAll<ArtifactType>().Any(type => type.Id == TypeId);

        if (!supported)
        {
            yield return new ValidationResult($"Unsupported artifact type ID '{TypeId}'.", new[] { nameof(TypeId) });
        }
    }
}