using Microsoft.EntityFrameworkCore;

namespace Espada.Db.Models;

[Owned]
public sealed class ImportFailureModel
{
    public string? Code { get; set; }
    
    public string? Reason { get; set; }
}
