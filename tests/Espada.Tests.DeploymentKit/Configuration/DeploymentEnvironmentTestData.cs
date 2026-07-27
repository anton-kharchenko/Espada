using Espada.DeploymentKit.Enums;
using Xunit;

namespace Espada.Tests.DeploymentKit.Configuration;

public static class DeploymentEnvironmentTestData
{
    public static TheoryData<string, DeploymentEnvironmentType> SupportedEnvironments => new()
    {
        { "dev", DeploymentEnvironmentType.Development },
        { "development", DeploymentEnvironmentType.Development },
        { "staging", DeploymentEnvironmentType.Staging },
        { "prod", DeploymentEnvironmentType.Production },
        { " PRODUCTION ", DeploymentEnvironmentType.Production }
    };

    public static TheoryData<string?> UnsupportedEnvironments =>
    [
        string.Empty,
        "qa",
        null!
    ];
}