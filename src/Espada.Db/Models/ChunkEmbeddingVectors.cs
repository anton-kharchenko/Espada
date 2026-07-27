using Espada.Db.Constants;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ChunkEmbeddingVectors, Schema = DbConstants.SchemaName)]
public class ChunkEmbeddingVectors
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ChunkEmbeddingId { get; set; }

    [Required, Column(TypeName = DbConstants.ColumnTypes.Numeric.Vector)]
    public Vector Vector { get; set; } = new(Array.Empty<float>());
}