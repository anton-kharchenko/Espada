using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.UsageReconciliationOutbox, Schema = DbConstants.SchemaName)]
    public sealed class UsageReconciliationOutbox
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid EventId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid LedgerEntryId { get; set; }

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
}