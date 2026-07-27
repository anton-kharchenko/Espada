using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.PaymentEvents, Schema = DbConstants.SchemaName)]
public sealed class PaymentEvents
{
    [Key]
    [MaxLength(255)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying255)]
    public string ProviderEventId { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying50)]
    public string ApiVersion { get; set; } = string.Empty;

    [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
    public string PayloadJson { get; set; } = string.Empty;

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset ProviderCreatedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset ReceivedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset AvailableAtUtc { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Attempt { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Status { get; set; }

    [MaxLength(200)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string? LeaseOwner { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? LeaseExpiresAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? ProcessedAtUtc { get; set; }

    [MaxLength(1000)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying1000)]
    public string? SanitizedError { get; set; }
}