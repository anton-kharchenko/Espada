using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.ImportStatusTypes, Schema = DbConstants.SchemaName)]
public class ImportStatusTypes
{
    [Key, Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int ImportStatusTypeId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L100), Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
    public string Name { get; set; } = null!;
}