using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.Sources, Schema = DbConstants.SchemaName)]
public class Sources
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid SourceId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L200), Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
    public string Name { get; set; } = null!;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int TypeId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L2048), Column(TypeName = DbTextColumnTypeConstants.Varchar2048)]
    public string Locator { get; set; } = null!;

    [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
    public string? DefinitionJson { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Priority { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset? ArchivedAtUtc { get; set; }

    public uint Version { get; set; }
}