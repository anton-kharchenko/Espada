using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.SyncConflicts, Schema = DbConstants.SchemaName)]
    public sealed class SyncConflicts
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid SyncConflictId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid WorkspaceId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying100)]
        public string EntityType { get; set; } = string.Empty;

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid EntityId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid LocalEventId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid RemoteEventId { get; set; }

        [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
        public string DetailsJson { get; set; } = "{}";

        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int StatusId { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset CreatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset? ResolvedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}
