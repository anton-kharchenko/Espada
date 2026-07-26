using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Espada.Db.Constants;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ChunkBatchStatusTypes, Schema = DbConstants.SchemaName)]
public class ChunkBatchStatusTypes
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int ChunkBatchStatusTypeId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L100), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar100)]
    public string Name { get; set; } = null!;
}
