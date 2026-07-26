using Espada.DeploymentKit.Configuration;
using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration;

public sealed class SensitiveConfigurationKeyClassifierTests
{
    [Theory]
    [InlineData("dbPassword")]
    [InlineData("Database:ConnectionString")]
    [InlineData("Stripe:SecretKey")]
    [InlineData("SendGridApiKey")]
    [InlineData("Auth__JwtToken")]
    [InlineData("Storage.AccessKey")]
    [InlineData("Google-ClientSecret")]
    [InlineData("SigningPrivateKey")]
    public void IsSensitive_ReturnsTrueForCredentialLikeKeys(string key)
    {
        Assert.True(SensitiveConfigurationKeyClassifier.IsSensitive(key));
    }

    [Theory]
    [InlineData("environment")]
    [InlineData("apiHost")]
    [InlineData("resourceGroupName")]
    [InlineData("imageTag")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsSensitive_ReturnsFalseForNonCredentialKeys(string? key)
    {
        Assert.False(SensitiveConfigurationKeyClassifier.IsSensitive(key));
    }
}
