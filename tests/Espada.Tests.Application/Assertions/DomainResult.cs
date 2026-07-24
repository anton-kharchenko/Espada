using Espada.Domain.Rules;

namespace Espada.Tests.Application.Assertions;

internal static class DomainResultAssertions
{
    public static void ShouldSucceed(this DomainResult result)
    {
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    public static TValue ShouldSucceed<TValue>(this DomainResult<TValue> result)
    {
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();

        return result.Value;
    }

    public static void ShouldFailWith(this DomainResult result, DomainError expectedError)
    {
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }

    public static void ShouldFailWith<TValue>(this DomainResult<TValue> result, DomainError expectedError)
    {
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }
}