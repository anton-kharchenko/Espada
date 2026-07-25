using System.ComponentModel.DataAnnotations;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;

namespace Espada.Tests.Api.Contracts.Requests.Imports;

public sealed class FailImportRequestTests
{
    [Fact]
    public void Validate_WithValidRequest_ShouldNotReturnErrors()
    {
        FailImportRequest request = new()
        {
            FailureCode = TestValues.ImportFailureCode,
            FailureReason = TestValues.ImportFailureReason
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Empty(results);
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyFailureCode_ShouldReturnFailureCodeError(string? failureCode)
    {
        FailImportRequest request = new()
        {
            FailureCode = failureCode!,
            FailureReason = TestValues.ImportFailureReason
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(FailImportRequest.FailureCode)));
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyFailureReason_ShouldReturnFailureReasonError(string? failureReason)
    {
        FailImportRequest request = new()
        {
            FailureCode = TestValues.ImportFailureCode,
            FailureReason = failureReason!
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(FailImportRequest.FailureReason)));
    }

    [Fact]
    public void Validate_WithFailureCodeTooLong_ShouldReturnFailureCodeError()
    {
        FailImportRequest request = new()
        {
            FailureCode = new string('a', FailImportRequest.FailureCodeMaxLength + 1),
            FailureReason = TestValues.ImportFailureReason
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(FailImportRequest.FailureCode)));
    }

    [Fact]
    public void Validate_WithFailureReasonTooLong_ShouldReturnFailureReasonError()
    {
        FailImportRequest request = new()
        {
            FailureCode = TestValues.ImportFailureCode,
            FailureReason = new string('a', FailImportRequest.FailureReasonMaxLength + 1)
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(FailImportRequest.FailureReason)));
    }
}