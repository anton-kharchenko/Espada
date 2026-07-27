using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Sources;

public sealed class RegisterSourceRequest : IValidatableObject
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(2048)]
    public string Locator { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TypeId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool supported = Enumeration.GetAll<SourceType>().Any(type => type.Id == TypeId);

        if (!supported)
        {
            yield return new ValidationResult($"Unsupported source type ID '{TypeId}'.", new[] { nameof(TypeId) });
        }
    }
}