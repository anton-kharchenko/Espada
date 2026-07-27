using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ChunkBatches, Schema = DbConstants.SchemaName)]
public class ChunkBatches
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkBatchId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ArtifactId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ArtifactRevisionId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int StrategyId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L64), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar64)]
    public string StrategyVersion { get; set; } = null!;

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset RequestedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset? StartedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset? CompletedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int? ChunkCount { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Text.TextType)]
    public string? FailureReason { get; set; }

    public uint Version { get; set; }
}