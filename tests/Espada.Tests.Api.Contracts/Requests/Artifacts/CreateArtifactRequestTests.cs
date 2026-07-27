using Espada.Api.Contracts.Requests.Artifacts;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.Artifacts;

public sealed class CreateArtifactRequestTests
{
    [Fact]
    public void Validate_WithValidRequest_ShouldNotReturnErrors()
    {
        ArtifactType artifactType = Enumeration.GetAll<ArtifactType>().First();

        CreateArtifactRequest request = new()
        {
            Title = TestValues.ArtifactTitle,
            TypeId = artifactType.Id,
            Content = TestValues.ArtifactContent
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Empty(results);
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyTitle_ShouldReturnTitleError(string? title)
    {
        ArtifactType artifactType = Enumeration.GetAll<ArtifactType>().First();

        CreateArtifactRequest request = new()
        {
            Title = title!,
            TypeId = artifactType.Id,
            Content = TestValues.ArtifactContent
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateArtifactRequest.Title)));
    }

    [Fact]
    public void Validate_WithUnsupportedTypeId_ShouldReturnTypeIdError()
    {
        CreateArtifactRequest request = new()
        {
            Title = TestValues.ArtifactTitle,
            TypeId = int.MaxValue,
            Content = TestValues.ArtifactContent
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateArtifactRequest.TypeId)));
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyContent_ShouldReturnContentError(string? content)
    {
        ArtifactType artifactType = Enumeration.GetAll<ArtifactType>().First();

        CreateArtifactRequest request = new()
        {
            Title = TestValues.ArtifactTitle,
            TypeId = artifactType.Id,
            Content = content!
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateArtifactRequest.Content)));
    }
}