using Espada.Api.Contracts.Requests.Context;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.Context;

public sealed class SearchWorkspaceContextRequestTests
{
    [Fact]
    public void Validate_WithEmptyVector_ShouldReturnVectorError()
    {
        SearchWorkspaceContextRequest request = new()
        {
            QueryText = "context",
            QueryVector = [],
            ModelIdentifier = "model",
            ModelVersion = "1"
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(SearchWorkspaceContextRequest.QueryVector)));
    }

    [Fact]
    public void Validate_WithInvalidLimits_ShouldReturnErrors()
    {
        SearchWorkspaceContextRequest request = new()
        {
            QueryText = "context",
            QueryVector = [1f],
            ModelIdentifier = "model",
            ModelVersion = "1",
            TopK = 101,
            MinimumSimilarity = 2,
            MinimumArtifactPriority = 101
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(SearchWorkspaceContextRequest.TopK)));
        Assert.True(results.HasErrorFor(nameof(SearchWorkspaceContextRequest.MinimumSimilarity)));
        Assert.True(results.HasErrorFor(nameof(SearchWorkspaceContextRequest.MinimumArtifactPriority)));
    }

    [Fact]
    public void Validate_WithNonFiniteVector_ShouldReturnVectorError()
    {
        SearchWorkspaceContextRequest request = new()
        {
            QueryText = "context",
            QueryVector = [float.NaN],
            ModelIdentifier = "model",
            ModelVersion = "1"
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(SearchWorkspaceContextRequest.QueryVector)));
    }
}