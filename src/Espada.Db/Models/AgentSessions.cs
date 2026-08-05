using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.AgentSessions, Schema = DbConstants.SchemaName)]
    public sealed class AgentSessions
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentSessionId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid WorkspaceId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ProjectId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentProfileId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid DeviceId { get; set; }

        [Column(TypeName = DbTextColumnTypeConstants.TextType)]
        public string Prompt { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L255)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying255)]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L2048)]
        [Column(TypeName = "character varying(2048)")]
        public string WorktreePath { get; set; } = string.Empty;

        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int StatusId { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset CreatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset UpdatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset? FinishedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}
