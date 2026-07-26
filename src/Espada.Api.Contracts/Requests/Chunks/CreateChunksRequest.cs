using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Chunks;

public sealed class CreateChunksRequest : IValidatableObject
{
    public IReadOnlyList<CreateChunkItemRequest> Items { get; init; } = Array.Empty<CreateChunkItemRequest>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Items.Count == 0)
        {
            yield return new ValidationResult("At least one chunk is required.", [nameof(Items)]);
            yield break;
        }

        if (Items.GroupBy(item => item.Number).Any(group => group.Count() > 1))
        {
            yield return new ValidationResult("Chunk numbers must be unique within a batch.", [nameof(Items)]);
        }

        for (int index = 0; index < Items.Count; index++)
        {
            CreateChunkItemRequest item = Items[index];
            List<ValidationResult> results = [];
            Validator.TryValidateObject(item, new ValidationContext(item), results, validateAllProperties: true);

            foreach (ValidationResult result in results)
            {
                string[] members = result.MemberNames.Select(member => $"{nameof(Items)}[{index}].{member}").ToArray();
                yield return new ValidationResult(result.ErrorMessage, members);
            }
        }
    }
}