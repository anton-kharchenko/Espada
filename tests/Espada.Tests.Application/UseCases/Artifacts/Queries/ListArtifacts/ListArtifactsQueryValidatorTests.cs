using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    public sealed class ListArtifactsQueryValidatorTests
    {
        private readonly ListArtifactsQueryValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
        {
            ListArtifactsQuery query =
                new ListArtifactsQueryBuilder().Build();

            TestValidationResult<ListArtifactsQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            ListArtifactsQuery query =
                new ListArtifactsQueryBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<ListArtifactsQuery> result =
                await _validator.TestValidateAsync(
                    query,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(query => query.WorkspaceId);
        }
    }
}