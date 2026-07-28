using Espada.Api.Contracts.Requests.ChunkBatches;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.ChunkBatches
{
    public sealed class CreateChunkBatchRequestTests
    {
        [Fact]
        public void Validate_WithUnsupportedStrategy_ShouldReturnStrategyError()
        {
            CreateChunkBatchRequest request = new() { StrategyId = int.MaxValue, StrategyVersion = "fixed-size-v1" };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CreateChunkBatchRequest.StrategyId)));
        }

        [Fact]
        public void Validate_WithStrategyVersionTooLong_ShouldReturnVersionError()
        {
            CreateChunkBatchRequest request = new()
            {
                StrategyId = Enumeration.GetAll<ChunkingStrategyType>().First().Id,
                StrategyVersion = new string('v', ChunkingVersion.MaxLength + 1)
            };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CreateChunkBatchRequest.StrategyVersion)));
        }
    }
}