using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Espada.Db.Models
{
    [Owned]
    public sealed class ImportFailureModel
    {
        [Column(TypeName = DbTextColumnTypeConstants.Varchar200)]
        public string? Code { get; set; }

        [Column(TypeName = DbTextColumnTypeConstants.Varchar4000)]
        public string? Reason { get; set; }
    }
}