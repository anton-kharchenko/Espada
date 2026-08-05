namespace Espada.Application.Models
{
    public sealed record RepositoryScanResult(
        string RepositoryRoot,
        IReadOnlyList<RepositoryFileRecord> Files);
}