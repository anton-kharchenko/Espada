using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ImportStatusTypes, Schema = DbConstants.SchemaName)]
public class ImportStatusTypes
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int ImportStatusTypeId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L100), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar100)]
    public string Name { get; set; } = null!;
}