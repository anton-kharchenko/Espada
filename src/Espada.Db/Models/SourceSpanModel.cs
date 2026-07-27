using Microsoft.EntityFrameworkCore;

namespace Espada.Db.Models;

[Owned]
public sealed class SourceSpanModel
{
    public int Start { get; set; }
    public int Length { get; set; }
}