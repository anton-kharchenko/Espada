namespace Espada.Api.LocalSetup.Models
{
    internal sealed record LocalSetupPreviewResponse(
        Guid SetupId,
        string RepositoryRoot,
        string WorkspaceName,
        string ProjectName,
        string? CanonicalRemoteUri,
        IReadOnlyList<LocalSetupInstructionPreview> Instructions,
        IReadOnlyList<LocalSetupAgentPreview> Agents,
        IReadOnlyList<LocalSetupMcpConfigurationPreview> McpConfigurations,
        LocalSetupPortPreview Ports,
        bool CloudLoginOptional);
}
