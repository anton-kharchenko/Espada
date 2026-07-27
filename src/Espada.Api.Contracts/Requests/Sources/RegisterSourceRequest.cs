using Espada.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Espada.Api.Contracts.Serialization;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Api.Contracts.Requests.Sources;

public sealed class RegisterSourceRequest : IValidatableObject
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [JsonConverter(typeof(SourceDefinitionJsonConverter))]
    public SourceDefinition? Definition { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }
    }
}