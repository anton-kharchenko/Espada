using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.InstructionRules, Schema = DbConstants.SchemaName)]
    public sealed class InstructionRules
    {
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid ArtifactRevisionId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L32)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar32)]
        public string Kind { get; set; } = string.Empty;

        [Required]
        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
        public string RuleKey { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = DbTextColumnTypeConstants.TextType)]
        public string Text { get; set; } = string.Empty;

        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int Priority { get; set; }
    }
}