using Espada.DeploymentKit.Helpers;
using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration;

public sealed class SensitiveConfigurationKeyClassifierHelperTests
{
    [Theory]
    [MemberData(nameof(SensitiveConfigurationKeyTestData.SensitiveKeys),MemberType = typeof(SensitiveConfigurationKeyTestData))]
    public void IsSensitive_ReturnsTrueForCredentialLikeKeys(string key) => Assert.True(SensitiveConfigurationKeyClassifierHelper.IsSensitive(key));

    [Theory]
    [MemberData(nameof(SensitiveConfigurationKeyTestData.NonSensitiveKeys), MemberType = typeof(SensitiveConfigurationKeyTestData))]
    public void IsSensitive_ReturnsFalseForNonCredentialKeys(string? key) => Assert.False(SensitiveConfigurationKeyClassifierHelper.IsSensitive(key));
}