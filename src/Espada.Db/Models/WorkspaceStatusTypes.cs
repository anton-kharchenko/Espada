using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Espada.Db.Constants;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.WorkspaceStatusTypes, Schema = DbConstants.SchemaName)]
public class WorkspaceStatusTypes
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int WorkspaceStatusTypeId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L100), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar100)]
    public string Name { get; set; } = null!;
}
