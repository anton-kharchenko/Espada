namespace Espada.Tests.Domain.ValueObjects
{
    public sealed class ContextPriorityTests
    {
        public static TheoryData<int> ValidValues => [-100, 0, 100];

        public static TheoryData<int> InvalidValues => [-101, 101];

        [Theory]
        [MemberData(nameof(ValidValues))]
        public void Create_WhenWithinRange_ShouldSucceed(int value)
        {
            ContextPriority.Create(value).ShouldSucceed().Value.Should().Be(value);
        }

        [Theory]
        [MemberData(nameof(InvalidValues))]
        public void Create_WhenOutsideRange_ShouldFail(int value)
        {
            ContextPriority.Create(value).IsFailure.Should().BeTrue();
        }
    }
}