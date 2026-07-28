namespace Espada.Infrastructure.Models
{
    internal sealed record ImportStageScheduledPayload(
        Guid ImportJobId,
        int StageId,
        DateTimeOffset ScheduledAtUtc);
}