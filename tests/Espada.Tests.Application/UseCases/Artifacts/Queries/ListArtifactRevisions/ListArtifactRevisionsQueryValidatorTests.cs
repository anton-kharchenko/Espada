using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    public sealed class ListArtifactRevisionsQueryValidatorTests
    {
        private readonly ListArtifactRevisionsQueryValidator _validator =
            new();

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
        {
            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder().Build();

            TestValidationResult<ListArtifactRevisionsQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<ListArtifactRevisionsQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(query => query.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyArtifactId_ShouldHaveError()
        {
            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .ForArtifact(Guid.Empty)
                    .Build();

            TestValidationResult<ListArtifactRevisionsQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(query => query.ArtifactId);
        }
    }
}