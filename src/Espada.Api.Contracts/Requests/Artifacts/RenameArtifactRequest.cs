using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Artifacts
{
    public sealed class RenameArtifactRequest : IValidatableObject
    {
        [Required] public string Title { get; init; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                yield return new ValidationResult("Title cannot be empty.", new[] { nameof(Title) });
            }
        }
    }
}