using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.MemoryMetadata, Schema = DbConstants.SchemaName)]
    public sealed class MemoryMetadataRecords
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid MemoryId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ArtifactId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ArtifactRevisionId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L32)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar32)]
        public string Kind { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L32)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar32)]
        public string Category { get; set; } = string.Empty;

        [Column(TypeName = DbNumericColumnTypeConstants.Numeric5_4)]
        public decimal Confidence { get; set; }

        [Column(TypeName = "boolean")] public bool UserConfirmed { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L200)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
        public string ClientIdentity { get; set; } = string.Empty;

        [MaxLength(DbMaxLengthConstants.L200)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
        public string? SessionIdentity { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset CapturedAtUtc { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid? SupersededMemoryId { get; set; }
    }
}