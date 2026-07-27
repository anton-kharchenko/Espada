using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.Chunks, Schema = DbConstants.SchemaName)]
public class Chunks
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkBatchId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ArtifactId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ArtifactRevisionId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int ChunkNumber { get; set; }

    [Required, Column(TypeName = DbConstants.ColumnTypes.Text.TextType)]
    public string Content { get; set; } = null!;

    public SourceSpanModel? SourceSpan { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int StrategyId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L64), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar64)]
    public string StrategyVersion { get; set; } = null!;

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }
}