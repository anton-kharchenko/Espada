using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.Tasks, Schema = DbConstants.SchemaName)]
    public sealed class Tasks
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid TaskId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid WorkspaceId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L500)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L32)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar32)]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset CreatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset UpdatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset? CompletedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset? ArchivedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}