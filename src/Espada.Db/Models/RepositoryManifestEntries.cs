using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.RepositoryManifestEntries, Schema = DbConstants.SchemaName)]
    public sealed class RepositoryManifestEntries
    {
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid SourceId { get; set; }

        [MaxLength(DbMaxLengthConstants.L2048)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar2048)]
        public string RelativePath { get; set; } = string.Empty;

        [MaxLength(DbMaxLengthConstants.L64)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar64)]
        public string ContentHash { get; set; } = string.Empty;

        [MaxLength(DbMaxLengthConstants.L255)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar255)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(DbMaxLengthConstants.L255)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar255)]
        public string MediaType { get; set; } = string.Empty;

        [Column(TypeName = DbNumericColumnTypeConstants.BigInt)]
        public long SizeInBytes { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset ScannedAtUtc { get; set; }
    }
}