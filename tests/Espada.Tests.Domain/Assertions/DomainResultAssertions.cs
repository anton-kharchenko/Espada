namespace Espada.Tests.Domain.Assertions
{
    internal static class DomainResultAssertions
    {
        public static void ShouldSucceed(this DomainResult result)
        {
            result.IsSuccess.Should()
                .BeTrue($"expected success, but received {result.Error.Code}: {result.Error.Description}");
        }

        public static TValue ShouldSucceed<TValue>(this DomainResult<TValue> result)
        {
            result.IsSuccess.Should()
                .BeTrue($"expected success, but received {result.Error.Code}: {result.Error.Description}");

            return result.Value;
        }

        public static void ShouldFailWith(this DomainResult result, DomainError expectedError)
        {
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(expectedError);
        }

        public static void ShouldFailWith<TValue>(this DomainResult<TValue> result, DomainError expectedError)
        {
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(expectedError);
        }
    }
}