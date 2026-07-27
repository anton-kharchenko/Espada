using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors;

public static class ImportJobErrors
{
    public static readonly DomainError CannotStart = new(
        "ImportJob.CannotStart",
        "Only a requested import job can be started.");

    public static readonly DomainError CannotComplete = new(
        "ImportJob.CannotComplete",
        "Only a running import job can be completed.");

    public static readonly DomainError CannotFail = new(
        "ImportJob.CannotFail",
        "Only a running import job can be marked as failed.");

    public static readonly DomainError CannotCancel = new(
        "ImportJob.CannotCancel",
        "Only a requested or running import job can be cancelled.");

    public static readonly DomainError CannotAdvanceStage = new(
        "ImportJob.CannotAdvanceStage",
        "Only the current pipeline stage can be completed.");

    public static readonly DomainError PipelineReferenceConflict = new(
        "ImportJob.PipelineReference.Conflict",
        "A pipeline stage attempted to replace an existing reference.");

    public static readonly DomainError FailureCodeEmpty = new(
        "ImportJob.Failure.Code.Empty",
        "Import failure code cannot be empty.");

    public static readonly DomainError FailureReasonEmpty = new(
        "ImportJob.Failure.Reason.Empty",
        "Import failure reason cannot be empty.");

    public static readonly DomainError FailureCodeTooLong = new(
        "ImportJob.Failure.Code.TooLong",
        $"Import failure code cannot exceed {ImportFailure.CodeMaxLength} characters.");

    public static readonly DomainError FailureReasonTooLong = new(
        "ImportJob.Failure.Reason.TooLong",
        $"Import failure reason cannot exceed {ImportFailure.ReasonMaxLength} characters.");
}