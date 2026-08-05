namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    public sealed record RequestImportResponse(
        Guid? ImportJobId,
        IReadOnlyList<Guid> WorkItemIds);
}