using Espada.Api.Contracts.Requests.ChunkEmbeddings;
using Espada.Domain.ValueObjects;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.ChunkEmbeddings
{
    public sealed class CreateChunkEmbeddingRequestTests
    {
        public static TheoryData<float> NonFiniteVectorValues =>
            new() { float.NaN, float.PositiveInfinity, float.NegativeInfinity };

        [Fact]
        public void Validate_WithEmptyVector_ShouldReturnVectorError()
        {
            CreateChunkEmbeddingRequest request = new() { ModelIdentifier = "test-model", ModelVersion = "1" };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CreateChunkEmbeddingRequest.Vector)));
        }

        [Fact]
        public void Validate_WithModelIdentifierTooLong_ShouldReturnIdentifierError()
        {
            CreateChunkEmbeddingRequest request =
                CreateValidRequest(new string('m', EmbeddingModel.IdentifierMaxLength + 1));

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CreateChunkEmbeddingRequest.ModelIdentifier)));
        }

        [Fact]
        public void Validate_WithModelVersionTooLong_ShouldReturnVersionError()
        {
            CreateChunkEmbeddingRequest request =
                CreateValidRequest(modelVersion: new string('v', EmbeddingModel.VersionMaxLength + 1));

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CreateChunkEmbeddingRequest.ModelVersion)));
        }

        [Theory]
        [MemberData(nameof(NonFiniteVectorValues))]
        public void Validate_WithNonFiniteVectorValue_ShouldReturnVectorError(float value)
        {
            CreateChunkEmbeddingRequest request = CreateValidRequest(vector: [value]);

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CreateChunkEmbeddingRequest.Vector)));
        }

        private static CreateChunkEmbeddingRequest CreateValidRequest(string? modelIdentifier = null,
            string? modelVersion = null, IReadOnlyList<float>? vector = null)
        {
            return new CreateChunkEmbeddingRequest
            {
                ModelIdentifier = modelIdentifier ?? "test-model",
                ModelVersion = modelVersion ?? "1",
                Vector = vector ?? [0.5f]
            };
        }
    }
}