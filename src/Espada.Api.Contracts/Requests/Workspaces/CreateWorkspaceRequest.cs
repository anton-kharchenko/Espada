using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Workspaces
{
    public sealed class CreateWorkspaceRequest
    {
        [Required][MaxLength(200)] public string Name { get; init; } = string.Empty;

        [Range(1, int.MaxValue)] public int TypeId { get; init; }

        public Guid? OrganizationId { get; init; }
    }
}