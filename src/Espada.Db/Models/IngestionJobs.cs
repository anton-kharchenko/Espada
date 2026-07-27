using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.IngestionJobs, Schema = DbConstants.SchemaName)]
public sealed class IngestionJobs
{
    [Key]
    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid JobId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid ImportJobId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Stage { get; set; }

    [MaxLength(200)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Attempt { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Status { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset AvailableAtUtc { get; set; }

    [MaxLength(200)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
    public string? LeaseOwner { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? LeaseExpiresAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? StartedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? CompletedAtUtc { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int? FailureCategory { get; set; }

    [MaxLength(4000)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying4000)]
    public string? SanitizedError { get; set; }
}