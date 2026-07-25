using System.ComponentModel.DataAnnotations;
using Espada.Api.Contracts.Requests.ChunkEmbeddings;
using Espada.Tests.Api.Contracts.Validation;

namespace Espada.Tests.Api.Contracts.Requests.ChunkEmbeddings;

public sealed class CreateChunkEmbeddingRequestTests
{
    [Fact]
    public void Validate_WithEmptyVector_ShouldReturnVectorError()
    {
        CreateChunkEmbeddingRequest request = new()
        {
            ModelIdentifier = "test-model",
            ModelVersion = "1"
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateChunkEmbeddingRequest.Vector)));
    }
}