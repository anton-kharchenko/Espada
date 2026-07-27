using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbTableConstants.BillingCustomers, Schema = DbConstants.SchemaName)]
public sealed class BillingCustomers
{
    [Key]
    [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
    public Guid WorkspaceId { get; set; }

    [MaxLength(255)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying255)]
    public string ProviderCustomerId { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying255)]
    public string? ProviderSubscriptionId { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Plan { get; set; }

    [MaxLength(100)]
    [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
    public string SubscriptionStatus { get; set; } = string.Empty;

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset? PaymentFailedAtUtc { get; set; }

    [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
    public DateTimeOffset LastProviderEventAtUtc { get; set; }
}