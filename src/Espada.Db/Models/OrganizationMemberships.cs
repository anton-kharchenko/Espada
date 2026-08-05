using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.OrganizationMemberships, Schema = DbConstants.SchemaName)]
    public sealed class OrganizationMemberships
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid OrganizationMembershipId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid OrganizationId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L500)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar500)]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L200)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L32)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar32)]
        public string Role { get; set; } = string.Empty;

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampTz)]
        public DateTimeOffset JoinedAtUtc { get; set; }
    }
}