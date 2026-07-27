using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.WorkspaceTypes, Schema = DbConstants.SchemaName)]
public class WorkspaceTypes
{
    [Key, Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int WorkspaceTypeId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L100), Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
    public string Name { get; set; } = null!;
}