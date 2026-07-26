using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration;

public sealed class DeploymentTargetHelperTests
{
    [Theory]
    [InlineData("website", DeploymentTargetType.Website)]
    [InlineData(" ALL ", DeploymentTargetType.All)]
    public void Parse_ReturnsTarget(string value, DeploymentTargetType expected) => Assert.Equal(expected, DeploymentTargetHelper.Parse(value));

    [Theory]
    [InlineData("")]
    [InlineData("api")]
    [InlineData(null)]
    public void Parse_RejectsUnsupportedTarget(string? value) => Assert.ThrowsAny<ArgumentException>(() => DeploymentTargetHelper.Parse(value!));
}