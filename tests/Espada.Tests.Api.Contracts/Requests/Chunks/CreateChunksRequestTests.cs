using System.ComponentModel.DataAnnotations;
using Espada.Api.Contracts.Requests.Chunks;
using Espada.Tests.Api.Contracts.Validation;

namespace Espada.Tests.Api.Contracts.Requests.Chunks;

public sealed class CreateChunksRequestTests
{
    [Fact]
    public void Validate_WithEmptyItems_ShouldReturnItemsError()
    {
        CreateChunksRequest request = new();

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateChunksRequest.Items)));
    }
}