using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.AgentApprovals, Schema = DbConstants.SchemaName)]
    public sealed class AgentApprovals
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentApprovalId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentSessionId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid RequestEventId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L200)]
        [Column(TypeName = DbTextColumnTypeConstants.CharacterVarying200)]
        public string ToolName { get; set; } = string.Empty;

        [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
        public string ArgumentsJson { get; set; } = "{}";

        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int StatusId { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset RequestedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset? DecidedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}
