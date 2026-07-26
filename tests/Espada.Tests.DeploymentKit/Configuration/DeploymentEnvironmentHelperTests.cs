using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Helpers;
using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration;

public sealed class DeploymentEnvironmentHelperTests
{
    [Theory]
    [MemberData(nameof(DeploymentEnvironmentTestData.SupportedEnvironments), MemberType = typeof(DeploymentEnvironmentTestData))]
    public void Parse_ReturnsEnvironment(string value, DeploymentEnvironmentType expected) => Assert.Equal(expected, DeploymentEnvironmentHelper.Parse(value));

    [Theory]
    [MemberData(nameof(DeploymentEnvironmentTestData.UnsupportedEnvironments), MemberType = typeof(DeploymentEnvironmentTestData))]
    public void Parse_RejectsUnsupportedEnvironment(string? value) => Assert.ThrowsAny<ArgumentException>(() => DeploymentEnvironmentHelper.Parse(value!));
}