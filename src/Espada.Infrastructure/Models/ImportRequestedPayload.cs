namespace Espada.Infrastructure.Models
{
    internal sealed record ImportRequestedPayload(
        Guid ImportJobId,
        Guid SourceId,
        Guid WorkspaceId,
        DateTimeOffset RequestedAtUtc);
}