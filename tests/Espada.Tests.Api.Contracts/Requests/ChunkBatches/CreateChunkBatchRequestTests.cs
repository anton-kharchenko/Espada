using System.ComponentModel.DataAnnotations;
using Espada.Api.Contracts.Requests.ChunkBatches;
using Espada.Tests.Api.Contracts.Validation;

namespace Espada.Tests.Api.Contracts.Requests.ChunkBatches;

public sealed class CreateChunkBatchRequestTests
{
    [Fact]
    public void Validate_WithUnsupportedStrategy_ShouldReturnStrategyError()
    {
        CreateChunkBatchRequest request = new()
        {
            StrategyId = int.MaxValue,
            StrategyVersion = "fixed-size-v1"
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateChunkBatchRequest.StrategyId)));
    }
}