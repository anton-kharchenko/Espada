using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    public sealed class GetArtifactByIdQueryValidatorTests
    {
        private readonly GetArtifactByIdQueryValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
        {
            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder().Build();

            TestValidationResult<GetArtifactByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<GetArtifactByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(
                query => query.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyArtifactId_ShouldHaveError()
        {
            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder()
                    .ForArtifact(Guid.Empty)
                    .Build();

            TestValidationResult<GetArtifactByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(
                query => query.ArtifactId);
        }
    }
}