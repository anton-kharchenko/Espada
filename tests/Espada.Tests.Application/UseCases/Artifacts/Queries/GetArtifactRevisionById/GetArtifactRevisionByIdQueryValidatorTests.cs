using Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById
{
    public sealed class GetArtifactRevisionByIdQueryValidatorTests
    {
        private readonly GetArtifactRevisionByIdQueryValidator _validator =
            new();

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
        {
            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder().Build();

            TestValidationResult<GetArtifactRevisionByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<GetArtifactRevisionByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(query => query.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyArtifactId_ShouldHaveError()
        {
            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .ForArtifact(Guid.Empty)
                    .Build();

            TestValidationResult<GetArtifactRevisionByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(query => query.ArtifactId);
        }

        [Fact]
        public async Task Validate_WithEmptyRevisionId_ShouldHaveError()
        {
            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .ForRevision(Guid.Empty)
                    .Build();

            TestValidationResult<GetArtifactRevisionByIdQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(query => query.ArtifactRevisionId);
        }
    }
}