using Espada.Api.Contracts.Requests.Sources;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Tests.Api.Contracts.TestData;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.Sources;

public sealed class RegisterSourceRequestTests
{
    [Fact]
    public void Validate_WithTypedDefinition_ShouldNotReturnErrors()
    {
        RegisterSourceRequest request = new()
        {
            Name = TestValues.SourceName,
            Definition = new PlainTextSourceDefinition("Notes", "Searchable content")
        };

        Assert.Empty(ValidationTestHelper.Validate(request));
    }

    [Fact]
    public void Validate_WithoutDefinition_ShouldReturnDefinitionError()
    {
        RegisterSourceRequest request = new() { Name = TestValues.SourceName };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterSourceRequest.Definition)));
    }

    [Fact]
    public void Validate_WithoutName_ShouldReturnNameError()
    {
        RegisterSourceRequest request = new()
        {
            Definition = new PlainTextSourceDefinition("Notes", "Searchable content")
        };

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterSourceRequest.Name)));
    }
}