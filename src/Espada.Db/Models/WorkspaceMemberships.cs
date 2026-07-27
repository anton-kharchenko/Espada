using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table("WorkspaceMemberships", Schema = "Espada")]
public sealed class WorkspaceMemberships
{
    [Key]
    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceMembershipId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying500)]
    public string Issuer { get; set; } = string.Empty;

    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string Subject { get; set; } = string.Empty;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Role { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset JoinedAtUtc { get; set; }
}