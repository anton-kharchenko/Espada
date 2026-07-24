namespace Espada.Tests.Domain.Assertions;

internal static class DomainResultAssertions
{
    public static TValue? ShouldSucceed<TValue>(
        this DomainResult<TValue> result)
    {
        result.IsSuccess
            .Should()
            .BeTrue($"expected success, but received {result.Error.Code}: {result.Error.Description}");

        return result.Value;
    }

    public static DomainError ShouldFailWith<TValue>(this DomainResult<TValue> result, DomainError expectedError)
    {
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);

        return result.Error;
    }
}