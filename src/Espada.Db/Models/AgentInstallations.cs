using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.AgentInstallations, Schema = DbConstants.SchemaName)]
    public sealed class AgentInstallations
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentInstallationId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid DeviceId { get; set; }

        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int VendorTypeId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L2048)]
        [Column(TypeName = "character varying(2048)")]
        public string ExecutablePath { get; set; } = string.Empty;

        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
        public string? DetectedVersion { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsAuthenticated { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset DetectedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}