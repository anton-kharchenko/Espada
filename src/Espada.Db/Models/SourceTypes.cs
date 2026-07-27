using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.SourceTypes, Schema = DbConstants.SchemaName)]
public class SourceTypes
{
    [Key, Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int SourceTypeId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L100), Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
    public string Name { get; set; } = null!;
}