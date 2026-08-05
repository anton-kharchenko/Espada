using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.Bindings, Schema = DbConstants.SchemaName)]
    public sealed class Bindings
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid BindingId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ArtifactRevisionId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid? OrganizationId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid WorkspaceId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid? ProjectId { get; set; }

        [MaxLength(DbMaxLengthConstants.L2048)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar2048)]
        public string? RepositoryCanonicalUri { get; set; }

        [MaxLength(DbMaxLengthConstants.L2000)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar2000)]
        public string? RepositoryRelativePathPrefix { get; set; }

        [MaxLength(DbMaxLengthConstants.L500)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar500)]
        public string? Branch { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid? TaskId { get; set; }

        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
        public string? Agent { get; set; }

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}