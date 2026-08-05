namespace Espada.LocalSetup.Contracts.Responses
{
    public sealed record LocalSetupMcpConfigurationPreview(
        string Agent,
        string Path,
        string Action);
}