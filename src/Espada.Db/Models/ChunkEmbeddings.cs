using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.ChunkEmbeddings, Schema = DbConstants.SchemaName)]
public class ChunkEmbeddings
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ChunkEmbeddingId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ChunkId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L64), Column(TypeName = DbTextColumnTypeConstants.Varchar64)]
    public string ChunkContentHash { get; set; } = null!;

    [Required, MaxLength(DbMaxLengthConstants.L200), Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
    public string ModelIdentifier { get; set; } = null!;

    [Required, MaxLength(DbMaxLengthConstants.L100), Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
    public string ModelVersion { get; set; } = null!;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Dimensions { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }
}