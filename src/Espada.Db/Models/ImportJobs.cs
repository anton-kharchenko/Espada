using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.ImportJobs, Schema = DbConstants.SchemaName)]
public class ImportJobs
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ImportJobId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid SourceId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int StatusId { get; set; }

    public int Stage { get; set; }

    [MaxLength(200)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [MaxLength(64)]
    public string RequestFingerprint { get; set; } = string.Empty;

    public string OptionsJson { get; set; } = "{}";

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset RequestedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset? StartedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset? CompletedAtUtc { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid? ArtifactId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid? ArtifactRevisionId { get; set; }

    public Guid? ChunkBatchId { get; set; }

    [MaxLength(200)]
    public string? RawBlobHash { get; set; }

    [MaxLength(200)]
    public string? ParsedBlobHash { get; set; }

    public ImportFailureModel? Failure { get; set; }

    public uint Version { get; set; }
}