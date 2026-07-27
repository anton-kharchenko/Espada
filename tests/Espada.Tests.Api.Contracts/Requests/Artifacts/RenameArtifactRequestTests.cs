using Espada.Api.Contracts.Requests.Artifacts;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.Artifacts;

public sealed class RenameArtifactRequestTests
{
    [Fact]
    public void Validate_WithValidRequest_ShouldNotReturnErrors()
    {
        RenameArtifactRequest request = new()
        {
            Title = TestValues.RenamedArtifactTitle
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Empty(results);
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyTitle_ShouldReturnTitleError(string? title)
    {
        RenameArtifactRequest request = new()
        {
            Title = title!
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(RenameArtifactRequest.Title)));
    }
}