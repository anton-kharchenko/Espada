using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration
{
    public static class SensitiveConfigurationKeyTestData
    {
        public static TheoryData<string> SensitiveKeys =>
        [
            "dbPassword",
            "Database:ConnectionString",
            "Stripe:SecretKey",
            "SendGridApiKey",
            "Auth__JwtToken",
            "Storage.AccessKey",
            "Google-ClientSecret",
            "SigningPrivateKey"
        ];

        public static TheoryData<string?> NonSensitiveKeys =>
        [
            "environmentType",
            "apiHost",
            "resourceGroupName",
            "imageTag",
            string.Empty,
            " ",
            null!
        ];
    }
}