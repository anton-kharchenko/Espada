using System.ComponentModel.DataAnnotations;

namespace Espada.Api.LocalSetup.Models
{
    internal sealed class CommitLocalSetupRequest
    {
        public Guid SetupId { get; init; }
        [Required] public string RepositoryPath { get; init; } = string.Empty;
        [Required][MaxLength(200)] public string WorkspaceName { get; init; } = string.Empty;
        [Required][MaxLength(200)] public string ProjectName { get; init; } = string.Empty;
        [Required] public string InitialInstruction { get; init; } = string.Empty;
        public int[] AgentVendorIds { get; init; } = [];
        public bool ConfigureMcp { get; init; } = true;
        public bool EnableCloudLogin { get; init; }
        [Range(1, 65535)] public int ApiPort { get; init; }
        [Range(1, 65535)] public int McpPort { get; init; }
        [Range(1, 65535)] public int PostgresPort { get; init; }
    }
}
