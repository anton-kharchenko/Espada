using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Espada.Db.Database;

public sealed class EspadaDbContextFactory : IDesignTimeDbContextFactory<EspadaDbContext>
{
    public EspadaDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = DbConfiguration.Create();

        return SetupDbContext.Create(configuration);
    }
}