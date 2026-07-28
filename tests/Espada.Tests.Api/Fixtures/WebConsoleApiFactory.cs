using Espada.Db.Constants;
using Espada.Db.Database;
using Espada.Db.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Espada.Tests.Api.Fixtures
{
    public sealed class WebConsoleApiFactory :
        WebApplicationFactory<Program>,
        IAsyncLifetime
    {
        public const string IdentityIssuer = "espada:test";

        public const string IdentitySubject = "test-user";

        private readonly PostgreSqlContainer _container =
            new PostgreSqlBuilder("pgvector/pgvector:0.8.2-pg17")
                .WithDatabase("espada_console_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        private NpgsqlDataSource? _dataSource;

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

        public HttpClient CreateConsoleClient()
        {
            return CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri("https://localhost"),
                    HandleCookies = true
                });
        }

        public async Task<Guid> SeedWorkspaceAsync(
            string name,
            CancellationToken cancellationToken,
            bool grantAccess = true)
        {
            Guid workspaceId = Guid.NewGuid();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            await using SetupDbContext dbContext = CreateSetupDbContext();
            await dbContext.Workspaces.AddAsync(
                new Workspaces
                {
                    WorkspaceId = workspaceId,
                    Name = name,
                    TypeId = 1,
                    StatusId = 1,
                    CreatedAtUtc = now
                },
                cancellationToken);
            if (grantAccess)
            {
                await dbContext.WorkspaceMemberships.AddAsync(
                    new WorkspaceMemberships
                    {
                        WorkspaceMembershipId = Guid.NewGuid(),
                        WorkspaceId = workspaceId,
                        Issuer = IdentityIssuer,
                        Subject = IdentitySubject,
                        Role = 1,
                        JoinedAtUtc = now
                    },
                    cancellationToken);
            }
            await dbContext.SaveChangesAsync(cancellationToken);

            return workspaceId;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("Testing")
                .UseContentRoot(AppContext.BaseDirectory)
                .UseSetting(
                    "ConnectionStrings:Espada",
                    _container.GetConnectionString())
                .UseSetting(
                    "WebConsole:Mode",
                    "Local")
                .UseSetting(
                    "WebConsole:LocalIdentityIssuer",
                    IdentityIssuer)
                .UseSetting(
                    "WebConsole:LocalIdentitySubject",
                    IdentitySubject);
            builder.ConfigureServices(services =>
            {
                services.Configure<HttpsRedirectionOptions>(options =>
                    options.HttpsPort = 7180);
                services.AddSingleton<
                    IStartupFilter,
                    LoopbackConnectionStartupFilter>();
            });
        }

        private SetupDbContext CreateSetupDbContext()
        {
            NpgsqlDataSource dataSource = _dataSource
                ?? throw new InvalidOperationException(
                    "The Web Console test database has not been initialized.");
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
