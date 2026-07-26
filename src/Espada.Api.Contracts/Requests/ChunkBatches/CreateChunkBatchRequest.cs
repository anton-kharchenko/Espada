using System.ComponentModel.DataAnnotations;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Api.Contracts.Requests.ChunkBatches;

public sealed class CreateChunkBatchRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int StrategyId { get; init; }

    [StringLength(ChunkingVersion.MaxLength)]
    public string StrategyVersion { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool supported = Enumeration.GetAll<ChunkingStrategyType>().Any(strategy => strategy.Id == StrategyId);

        if (!supported)
        {
            yield return new ValidationResult($"Unsupported chunking strategy ID '{StrategyId}'.", [nameof(StrategyId)]);
        }

        if (string.IsNullOrWhiteSpace(StrategyVersion))
        {
            yield return new ValidationResult("Chunking strategy version cannot be empty.", [nameof(StrategyVersion)]);
        }
    }
}