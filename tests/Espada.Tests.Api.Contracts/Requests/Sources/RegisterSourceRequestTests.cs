using System.ComponentModel.DataAnnotations;
using Espada.Api.Contracts.Requests.Sources;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;

namespace Espada.Tests.Api.Contracts.Requests.Sources;

public sealed class RegisterSourceRequestTests
{
    [Fact]
    public void Validate_WithValidRequest_ShouldNotReturnErrors()
    {
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        RegisterSourceRequest request = new()
        {
            Name = TestValues.SourceName,
            Locator = TestValues.SourceLocator,
            TypeId = sourceType.Id
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithUnsupportedTypeId_ShouldReturnTypeIdError()
    {
        RegisterSourceRequest request = new()
        {
            Name = TestValues.SourceName,
            Locator = TestValues.SourceLocator,
            TypeId = int.MaxValue
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        ValidationResult error = Assert.Single(results, result => result.MemberNames.Contains(nameof(RegisterSourceRequest.TypeId)));

        Assert.Equal($"Unsupported source type ID '{request.TypeId}'.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_WithZeroTypeId_ShouldReturnTypeIdError()
    {
        RegisterSourceRequest request = new()
        {
            Name = TestValues.SourceName,
            Locator = TestValues.SourceLocator,
            TypeId = 0
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(RegisterSourceRequest.TypeId)));
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyName_ShouldReturnNameError(string? name)
    {
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        RegisterSourceRequest request = new()
        {
            Name = name!,
            Locator = TestValues.SourceLocator,
            TypeId = sourceType.Id
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(RegisterSourceRequest.Name)));
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldReturnNameError()
    {
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        RegisterSourceRequest request = new()
        {
            Name = new string('a', 201),
            Locator = TestValues.SourceLocator,
            TypeId = sourceType.Id
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(RegisterSourceRequest.Name)));
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Validate_WithEmptyLocator_ShouldReturnLocatorError(string? locator)
    {
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        RegisterSourceRequest request = new()
        {
            Name = TestValues.SourceName,
            Locator = locator!,
            TypeId = sourceType.Id
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(RegisterSourceRequest.Locator)));
    }

    [Fact]
    public void Validate_WithLocatorTooLong_ShouldReturnLocatorError()
    {
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        RegisterSourceRequest request = new()
        {
            Name = TestValues.SourceName,
            Locator = new string('a', 2049),
            TypeId = sourceType.Id
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(RegisterSourceRequest.Locator)));
    }

    [Fact]
    public void Validate_WithEverySupportedSourceType_ShouldNotReturnTypeIdError()
    {
        IReadOnlyCollection<SourceType> sourceTypes = Enumeration.GetAll<SourceType>().ToArray();

        Assert.NotEmpty(sourceTypes);

        foreach (SourceType sourceType in sourceTypes)
        {
            RegisterSourceRequest request = new()
            {
                Name = TestValues.SourceName,
                Locator = TestValues.SourceLocator,
                TypeId = sourceType.Id
            };

            IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

            Assert.False(results.HasErrorFor(nameof(RegisterSourceRequest.TypeId)));
        }
    }
}