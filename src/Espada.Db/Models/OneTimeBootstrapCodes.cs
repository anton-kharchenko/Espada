using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.OneTimeBootstrapCodes, Schema = DbConstants.SchemaName)]
    public sealed class OneTimeBootstrapCodes
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid OneTimeBootstrapCodeId { get; set; }

        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying64)]
        public string CodeHash { get; set; } = string.Empty;

        [Column(TypeName = DbTextColumnTypeConstants.Varchar32)]
        public string Purpose { get; set; } = string.Empty;

        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying500)]
        public string IdentityIssuer { get; set; } = string.Empty;

        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
        public string IdentitySubject { get; set; } = string.Empty;

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset CreatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset ExpiresAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset? ConsumedAtUtc { get; set; }
    }
}