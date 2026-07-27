using Espada.Api.Contracts.Requests.Chunks;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

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

    [Fact]
    public void Validate_WithDuplicateNumbers_ShouldReturnItemsError()
    {
        CreateChunksRequest request = new()
        {
            Items =
            [
                new CreateChunkItemRequest { Number = 1, Content = "first" },
                new CreateChunkItemRequest { Number = 1, Content = "second" }
            ]
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateChunksRequest.Items)));
    }
}