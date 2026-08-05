using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Imports.Queries.GetImportById
{
    public sealed class GetImportByIdQueryValidatorTests
    {
        private readonly GetImportByIdQueryValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
        {
            // Arrange
            GetImportByIdQuery query = new GetImportByIdQueryBuilder().Build();

            // Act
            TestValidationResult<GetImportByIdQuery> result =
                await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<GetImportByIdQuery> result =
                await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(getImportByIdQuery => getImportByIdQuery.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyImportJobId_ShouldHaveError()
        {
            // Arrange
            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<GetImportByIdQuery> result =
                await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(getImportByIdQuery => getImportByIdQuery.ImportJobId);
        }
    }
}