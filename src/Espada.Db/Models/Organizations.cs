using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.Organizations, Schema = DbConstants.SchemaName)]
    public sealed class Organizations
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid OrganizationId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L200)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset CreatedAtUtc { get; set; }

        [Timestamp]
        [Column("xmin", TypeName = DbIdentifierColumnTypeConstants.Xid)]
        public uint Version { get; set; }
    }
}