using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Tests.Infrastructure.Database
{
    public sealed class EspadaDbContextTests
    {
        [Fact]
        public void Constructor_WithOptions_ShouldCreateContext()
        {
            DbContextOptions<EspadaDbContext> options = new DbContextOptionsBuilder<EspadaDbContext>().Options;

            using EspadaDbContext context = new(options);

            Assert.NotNull(context);
        }
    }
}