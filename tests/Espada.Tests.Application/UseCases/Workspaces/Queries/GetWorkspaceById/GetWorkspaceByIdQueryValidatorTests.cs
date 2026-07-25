using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Tests.Application.TestData;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Workspaces.Queries.GetWorkspaceById
{
    public sealed class GetWorkspaceByIdQueryValidatorTests
    {
        private readonly GetWorkspaceByIdQueryValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidWorkspaceId_ShouldNotHaveErrors()
        {
            // Arrange
            GetWorkspaceByIdQuery query = new(TestIds.WorkspaceId.Value);

            // Act
            TestValidationResult<GetWorkspaceByIdQuery> result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            GetWorkspaceByIdQuery query = new(Guid.Empty);

            // Act
            TestValidationResult<GetWorkspaceByIdQuery> result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);
        }
    }
}