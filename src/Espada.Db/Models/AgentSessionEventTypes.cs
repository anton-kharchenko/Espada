using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.AgentSessionEventTypes, Schema = DbConstants.SchemaName)]
    public sealed class AgentSessionEventTypes
    {
        [Key]
        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int AgentSessionEventTypeId { get; set; }

        [Required]
        [MaxLength(DbMaxLengthConstants.L100)]
        [Column(TypeName = DbTextColumnTypeConstants.Varchar100)]
        public string Name { get; set; } = string.Empty;
    }
}
