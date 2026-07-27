using Microsoft.EntityFrameworkCore;
using Espada.Db.Constants;
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