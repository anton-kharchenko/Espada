using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models;

[Owned]
public sealed class SourceSpanModel
{
    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Start { get; set; }

    [Column(TypeName = DbNumericColumnTypeConstants.Integer)]
    public int Length { get; set; }
}