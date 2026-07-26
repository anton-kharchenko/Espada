using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Espada.Db.Constants;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ChunkEmbeddingVectors, Schema = DbConstants.SchemaName)]
public class ChunkEmbeddingVectors
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkEmbeddingId { get; set; }

    [Required, Column(TypeName = DbConstants.ColumnTypes.Numeric.RealArray)]
    public float[] Vector { get; set; } = [];
}
