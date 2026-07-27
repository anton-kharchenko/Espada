using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.Artifacts, Schema = DbConstants.SchemaName)]
public class Artifacts
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Required, MaxLength(DbMaxLengthConstants.L200), Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
    public string Title { get; set; } = null!;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int TypeId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Priority { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid? CurrentRevisionId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int? CurrentRevisionNumber { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset? ArchivedAtUtc { get; set; }

    public uint Version { get; set; }
}