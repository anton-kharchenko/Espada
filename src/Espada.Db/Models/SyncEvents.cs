using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.SyncEvents, Schema = DbConstants.SchemaName)]
    public sealed class SyncEvents
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid EventId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid DeviceId { get; set; }

        [Column(TypeName = DbNumericColumnTypeConstants.BigInt)]
        public long Sequence { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid WorkspaceId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
        public string EntityType { get; set; } = string.Empty;

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid EntityId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L32)]
        [Column(TypeName = "character varying(32)")]
        public string Operation { get; set; } = string.Empty;

        [Column(TypeName = DbNumericColumnTypeConstants.BigInt)]
        public uint? BaseVersion { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset OccurredAtUtc { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
        public string PayloadType { get; set; } = string.Empty;

        [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
        public string PayloadJson { get; set; } = "{}";

        [Required]
        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
        public string PayloadHash { get; set; } = string.Empty;
    }
}
