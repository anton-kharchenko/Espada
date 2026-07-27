using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration;

public sealed class DeploymentTargetHelperTests
{
    public static TheoryData<string, DeploymentTargetType> SupportedTargets =>
        new()
        {
            { "website", DeploymentTargetType.Website },
            { " ALL ", DeploymentTargetType.All }
        };

    public static TheoryData<string?> UnsupportedTargets =>
        new()
        {
            string.Empty,
            "api",
            null!
        };

    [Theory]
    [MemberData(nameof(SupportedTargets))]
    public void Parse_ReturnsTarget(string value, DeploymentTargetType expected) => Assert.Equal(expected, DeploymentTargetHelper.Parse(value));

    [Theory]
    [MemberData(nameof(UnsupportedTargets))]
    public void Parse_RejectsUnsupportedTarget(string? value) => Assert.ThrowsAny<ArgumentException>(() => DeploymentTargetHelper.Parse(value!));
}