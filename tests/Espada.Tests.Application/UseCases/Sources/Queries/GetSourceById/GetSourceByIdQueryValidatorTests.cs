using Espada.Application.UseCases.Sources.Queries.GetSourceById;
using Espada.Tests.Application.TestData;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Sources.Queries.GetSourceById
{
    public sealed class GetSourceByIdQueryValidatorTests
    {
        private readonly GetSourceByIdQueryValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
        {
            // Arrange
            GetSourceByIdQuery query = new(TestIds.DefaultWorkspaceId.Value, TestIds.SourceId.Value);

            // Act
            TestValidationResult<GetSourceByIdQuery> result =
                await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            GetSourceByIdQuery query = new(Guid.Empty, TestIds.SourceId.Value);

            // Act
            TestValidationResult<GetSourceByIdQuery> result =
                await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptySourceId_ShouldHaveError()
        {
            // Arrange
            GetSourceByIdQuery query = new(TestIds.DefaultWorkspaceId.Value, Guid.Empty);

            // Act
            TestValidationResult<GetSourceByIdQuery> result =
                await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.SourceId);
        }
    }
}