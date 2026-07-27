using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.ChunkingStrategyTypes, Schema = DbConstants.SchemaName)]
public class ChunkingStrategyTypes
{
    [Key, Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int ChunkingStrategyTypeId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L100), Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
    public string Name { get; set; } = null!;
}