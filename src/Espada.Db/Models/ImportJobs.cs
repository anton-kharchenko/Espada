using Espada.Db.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Table(DbConstants.Tables.ImportJobs, Schema = DbConstants.SchemaName)]
public class ImportJobs
{
    [Key, Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid ImportJobId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid SourceId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid WorkspaceId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Numeric.Integer)]
    public int StatusId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset RequestedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset? StartedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.DateTime.TimestampTz)]
    public DateTimeOffset? CompletedAtUtc { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid? ArtifactId { get; set; }

    [Column(TypeName = DbConstants.ColumnTypes.Identifier.Uuid)]
    public Guid? ArtifactRevisionId { get; set; }

    public ImportFailureModel? Failure { get; set; }

    public uint Version { get; set; }
}