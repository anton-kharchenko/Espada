using Espada.Db.Constants;
using Espada.Db.Database;
using Espada.Db.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenIddict.EntityFrameworkCore.Models;
using System.Security.Cryptography;
using System.Text;
using Testcontainers.PostgreSql;
using McpProgram = Espada.Mcp.Program;

namespace Espada.Tests.Mcp.Http
{
    public sealed class McpFactory :
        WebApplicationFactory<McpProgram>,
        IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container =
            new PostgreSqlBuilder("pgvector/pgvector:0.8.2-pg17")
                .WithDatabase("espada_mcp_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        private NpgsqlDataSource? _dataSource;

        public string ConnectionString => _container.GetConnectionString();

        public async ValueTask InitializeAsync()
        {
            await _container.StartAsync();
            NpgsqlDataSourceBuilder dataSourceBuilder =
                new(_container.GetConnectionString());
            dataSourceBuilder.UseVector();
            _dataSource = dataSourceBuilder.Build();

            await using SetupDbContext dbContext = CreateSetupDbContext();
            await dbContext.Database.MigrateAsync(
                TestContext.Current.CancellationToken);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (_dataSource is not null)
            {
                await _dataSource.DisposeAsync();
            }

            await _container.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public async Task<string> CreateAuthorityBootstrapCodeAsync(
            CancellationToken cancellationToken)
        {
            string code = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
            await using SetupDbContext dbContext = CreateSetupDbContext();
            await dbContext.OneTimeBootstrapCodes.AddAsync(
                new OneTimeBootstrapCodes
                {
                    OneTimeBootstrapCodeId = Guid.NewGuid(),
                    CodeHash = Convert.ToHexString(
                        SHA256.HashData(
                            Encoding.UTF8.GetBytes(code))),
                    Purpose = "mcp_authority",
                    IdentityIssuer = "espada:test",
                    IdentitySubject = "test-user",
                    CreatedAtUtc = createdAtUtc,
                    ExpiresAtUtc = createdAtUtc.AddMinutes(5)
                },
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return code;
        }

        public async Task<TimeSpan> GetLatestTokenLifetimeAsync(
            string tokenType,
            CancellationToken cancellationToken)
        {
            await using SetupDbContext dbContext = CreateSetupDbContext();
            OpenIddictEntityFrameworkCoreToken<Guid>? token =
                await dbContext
                    .Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
                    .AsNoTracking()
                    .Where(candidate =>
                        candidate.Type == tokenType
                        || candidate.Type != null
                        && candidate.Type.EndsWith($":{tokenType}"))
                    .OrderByDescending(candidate => candidate.CreationDate)
                    .FirstOrDefaultAsync(cancellationToken);
            if (token?.CreationDate is null
                || token.ExpirationDate is null)
            {
                string[] availableTypes = await dbContext
                    .Set<OpenIddictEntityFrameworkCoreToken<Guid>>()
                    .AsNoTracking()
                    .Select(candidate => candidate.Type ?? "(null)")
                    .Distinct()
                    .Order()
                    .ToArrayAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"No persisted '{tokenType}' token lifetime was found. "
                    + $"Available token types: {string.Join(", ", availableTypes)}.");
            }

            return token.ExpirationDate.Value - token.CreationDate.Value;
        }

        public HttpClient CreateOAuthClient()
        {
            return CreateClient(
                new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("Testing")
                .UseContentRoot(AppContext.BaseDirectory)
                .UseSetting(
                    "ConnectionStrings:Espada",
                    _container.GetConnectionString());
        }

        private SetupDbContext CreateSetupDbContext()
        {
            NpgsqlDataSource dataSource = _dataSource
                ?? throw new InvalidOperationException(
                    "The MCP test database has not been initialized.");
            DbContextOptionsBuilder<SetupDbContext> options = new();
            options.UseNpgsql(
                dataSource,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(
                        typeof(SetupDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        DbConstants.SchemaName);
                    npgsql.UseVector();
                });
            return new SetupDbContext(options.Options);
        }
    }
}
