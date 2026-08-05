using Espada.Api.Contracts.Requests.ArtifactRevisions;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.ArtifactRevisions
{
    public sealed class AddArtifactRevisionRequestTests
    {
        [Fact]
        public void Validate_WithValidRequest_ShouldNotReturnErrors()
        {
            AddArtifactRevisionRequest request = new() { Content = TestValues.ArtifactRevisionContent };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public void Validate_WithEmptyContent_ShouldReturnContentError(string? content)
        {
            AddArtifactRevisionRequest request = new() { Content = content! };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(AddArtifactRevisionRequest.Content)));
        }
    }
}