using Espada.Application.Enums;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Jobs;

public sealed record IngestionJob(
    Guid JobId,
    ImportJobId ImportJobId,
    ImportPipelineStageType Stage,
    string IdempotencyKey,
    int Attempt,
    IngestionJobStatusType Status,
    DateTimeOffset AvailableAtUtc,
    string? LeaseOwner,
    DateTimeOffset? LeaseExpiresAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    JobFailureCategoryType? FailureCategory = null,
    string? SanitizedError = null);