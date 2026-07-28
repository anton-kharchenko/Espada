using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Imports
{
    public sealed class FailImportRequest : IValidatableObject
    {
        public const int FailureCodeMaxLength = 200;
        public const int FailureReasonMaxLength = 4000;

        [MaxLength(FailureCodeMaxLength)] public string FailureCode { get; init; } = string.Empty;

        [MaxLength(FailureReasonMaxLength)] public string FailureReason { get; init; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FailureCode))
            {
                yield return new ValidationResult("Failure code cannot be empty.", new[] { nameof(FailureCode) });
            }

            if (string.IsNullOrWhiteSpace(FailureReason))
            {
                yield return new ValidationResult("Failure reason cannot be empty.", new[] { nameof(FailureReason) });
            }
        }
    }
}