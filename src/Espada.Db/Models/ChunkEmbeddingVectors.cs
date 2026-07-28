using Espada.Db.Constants;
using Pgvector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.ChunkEmbeddingVectors, Schema = DbConstants.SchemaName)]
    public class ChunkEmbeddingVectors
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ChunkEmbeddingId { get; set; }

        [Required]
        [Column(TypeName = DbNumericColumnTypeConstants.Vector)]
        public Vector Vector { get; set; } = new(Array.Empty<float>());
    }
}