using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.ChunkBatches, Schema = DbConstants.SchemaName)]
public class ChunkBatches
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ChunkBatchId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactRevisionId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int StrategyId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L64), Column(TypeName = DbTextColumnTypeConstants.Varchar64)]
    public string StrategyVersion { get; set; } = null!;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset RequestedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset? StartedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset? CompletedAtUtc { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int? ChunkCount { get; set; }

    [Column(TypeName = DbTextColumnTypeConstants.TextType)]
    public string? FailureReason { get; set; }

    public uint Version { get; set; }
}