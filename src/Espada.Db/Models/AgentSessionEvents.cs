using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Table(DbTableConstants.AgentSessionEvents, Schema = DbConstants.SchemaName)]
    public sealed class AgentSessionEvents
    {
        [Key]
        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentSessionEventId { get; set; }

        [Column(TypeName = DbIdentifierColumnTypeConstants.Uuid)]
        public Guid AgentSessionId { get; set; }

        [Column(TypeName = DbNumericColumnTypeConstants.BigInt)]
        public long Sequence { get; set; }

        [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
        public int TypeId { get; set; }

        [Column(TypeName = DbJsonColumnTypeConstants.Jsonb)]
        public string PayloadJson { get; set; } = "{}";

        [Column(TypeName = DbDateTimeColumnTypeConstants.TimestampWithTimeZone)]
        public DateTimeOffset OccurredAtUtc { get; set; }
    }
}