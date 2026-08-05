using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.ArtifactRevisions
{
    public sealed class AddArtifactRevisionRequest : IValidatableObject
    {
        [Required] public string Content { get; init; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                yield return new ValidationResult("Content cannot be empty.", new[] { nameof(Content) });
            }
        }
    }
}