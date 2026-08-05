using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.Projects, Schema = DbConstants.SchemaName)]
    public sealed class Projects
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ProjectId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid WorkspaceId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L200)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L2048)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar2048)]
        public string CanonicalRemoteUri { get; set; } = string.Empty;

        [Column(TypeName = DbTextColumnTypeConstants.TextArray)]
        public string[] LocalAliases { get; set; } = [];

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset CreatedAtUtc { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset UpdatedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}