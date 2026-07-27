using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.Workspaces, Schema = DbConstants.SchemaName)]
public class Workspaces
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L200), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar200)]
    public string Name { get; set; } = null!;

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int TypeId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset? ArchivedAtUtc { get; set; }

    public uint Version { get; set; }
}