using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.UsageLedgerEntries, Schema = DbConstants.SchemaName)]
public sealed class UsageLedgerEntries
{
    [Key]
    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid EntryId { get; set; }

    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [MaxLength(100)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
    public string Metric { get; set; } = string.Empty;

    [Column(TypeName = DbNumericColumnTypeConstants.BigInt)]
    public long Quantity { get; set; }

    [MaxLength(255)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying255)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset OccurredAtUtc { get; set; }
}