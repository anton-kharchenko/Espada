using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Espada.Db.Constants;

namespace Espada.Db.Models;

[Table(DbTableConstants.BillingCustomers, Schema = DbConstants.SchemaName)]
public sealed class BillingCustomers
{
    [Key]
    public Guid WorkspaceId { get; set; }

    [MaxLength(255)]
    public string ProviderCustomerId { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ProviderSubscriptionId { get; set; }

    public int Plan { get; set; }

    [MaxLength(100)]
    public string SubscriptionStatus { get; set; } = string.Empty;

    public DateTimeOffset? PaymentFailedAtUtc { get; set; }

    public DateTimeOffset LastProviderEventAtUtc { get; set; }
}