using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Espada.Db.Constants;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.Sources, Schema = DbConstants.SchemaName)]
public class Sources
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid SourceId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L200), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar200)]
    public string Name { get; set; } = null!;

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int TypeId { get; set; }

    [Required, MaxLength(DbConstants.Validations.MaxLengths.L2048), Column(TypeName = DbConstants.ColumnTypes.Text.Varchar2048)]
    public string Locator { get; set; } = null!;

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset? ArchivedAtUtc { get; set; }

    [ConcurrencyCheck, Column(TypeName = DbConstants.ColumnTypes.Numeric.BigInt)]
    public long Version { get; set; } = 1;
}
