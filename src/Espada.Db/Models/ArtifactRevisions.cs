using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.ArtifactRevisions, Schema = DbConstants.SchemaName)]
public class ArtifactRevisions
{
    [Key, Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactRevisionId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ArtifactId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int RevisionNumber { get; set; }

    [Required, Column(TypeName = DbTextColumnTypeConstants.TextType)]
    public string Content { get; set; } = null!;

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
    public DateTimeOffset CreatedAtUtc { get; set; }
}