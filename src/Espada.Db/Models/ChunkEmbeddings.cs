using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ChunkEmbeddings, Schema = DbConstants.SchemaName)]
public class ChunkEmbeddings
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkEmbeddingId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L64), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar64)]
    public string ChunkContentHash { get; set; } = null!;

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L200), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar200)]
    public string ModelIdentifier { get; set; } = null!;

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L100), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar100)]
    public string ModelVersion { get; set; } = null!;

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int Dimensions { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }
}