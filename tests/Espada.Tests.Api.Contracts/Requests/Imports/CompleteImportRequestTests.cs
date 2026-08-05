using Espada.Api.Contracts.Requests.Imports;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.Imports
{
    public sealed class CompleteImportRequestTests
    {
        [Fact]
        public void Validate_WithValidRequest_ShouldNotReturnErrors()
        {
            CompleteImportRequest request = new()
            {
                ArtifactId = TestIds.DefaultArtifactId,
                ArtifactRevisionId = TestIds.DefaultArtifactRevisionId
            };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_WithEmptyArtifactId_ShouldReturnArtifactIdError()
        {
            CompleteImportRequest request = new()
            {
                ArtifactId = Guid.Empty,
                ArtifactRevisionId = TestIds.DefaultArtifactRevisionId
            };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CompleteImportRequest.ArtifactId)));
        }

        [Fact]
        public void Validate_WithEmptyArtifactRevisionId_ShouldReturnArtifactRevisionIdError()
        {
            CompleteImportRequest request = new()
            {
                ArtifactId = TestIds.DefaultArtifactId,
                ArtifactRevisionId = Guid.Empty
            };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.True(results.HasErrorFor(nameof(CompleteImportRequest.ArtifactRevisionId)));
        }
    }
}