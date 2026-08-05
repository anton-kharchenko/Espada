using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Imports
{
    public sealed class RequestImportRequest : IValidatableObject
    {
        public Guid SourceId { get; init; }

        public ImportOptionsRequest? Options { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SourceId == Guid.Empty)
            {
                yield return new ValidationResult("SourceId is required.", [nameof(SourceId)]);
            }
        }
    }
}