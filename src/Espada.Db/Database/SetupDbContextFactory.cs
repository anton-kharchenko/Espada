using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Espada.Db.Database;

public sealed class SetupDbContextFactory : IDesignTimeDbContextFactory<SetupDbContext>
{
    public SetupDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = DbConfiguration.Create();
        return SetupDbContext.Create(configuration);
    }
}
