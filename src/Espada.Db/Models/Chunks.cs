using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.Chunks, Schema = DbConstants.SchemaName)]
public class Chunks
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ChunkId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ChunkBatchId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactRevisionId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int ChunkNumber { get; set; }

    [Required, Column(TypeName = DbTextColumnTypeConstants.TextType)]
    public string Content { get; set; } = null!;

    public SourceSpanModel? SourceSpan { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int StrategyId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L64), Column(TypeName = DbTextColumnTypeConstants.Varchar64)]
    public string StrategyVersion { get; set; } = null!;

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }
}