using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.OutboxMessages, Schema = DbConstants.SchemaName)]
public sealed class OutboxMessages
{
    [Key]
    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid EventId { get; set; }

    [MaxLength(200)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string EventName { get; set; } = string.Empty;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int EventVersion { get; set; }

    [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
    public string PayloadJson { get; set; } = string.Empty;

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset OccurredAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset AvailableAtUtc { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Attempt { get; set; }

    [MaxLength(200)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string? LeaseOwner { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? LeaseExpiresAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? ProcessedAtUtc { get; set; }

    [MaxLength(4000)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying4000)]
    public string? SanitizedError { get; set; }
}