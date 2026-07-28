using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Infrastructure;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Repositories;
using Espada.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Infrastructure.Repositories
{
    public sealed class RepositoryRegistrationTests
    {
        private const string ConnectionString =
            "Host=localhost;Port=5432;Database=espada_registration_tests;Username=postgres;Password=postgres";

        [Fact]
        public void AddInfrastructure_ShouldRegisterRepositories()
        {
            ServiceCollection services = new();

            services.AddInfrastructure(ConnectionString);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();

            IServiceProvider scopedServices = scope.ServiceProvider;

            Assert.IsType<WorkspaceRepository>(scopedServices.GetRequiredService<IWorkspaceRepository>());
            Assert.IsType<SourceRepository>(scopedServices.GetRequiredService<ISourceRepository>());
            Assert.IsType<ImportJobRepository>(scopedServices.GetRequiredService<IImportJobRepository>());
            Assert.IsType<ArtifactRepository>(scopedServices.GetRequiredService<IArtifactRepository>());
            Assert.IsType<ArtifactRevisionRepository>(scopedServices.GetRequiredService<IArtifactRevisionRepository>());
            Assert.IsType<ChunkBatchRepository>(scopedServices.GetRequiredService<IChunkBatchRepository>());
            Assert.IsType<ChunkRepository>(scopedServices.GetRequiredService<IChunkRepository>());
            Assert.IsType<ChunkEmbeddingRepository>(scopedServices.GetRequiredService<IChunkEmbeddingRepository>());
            Assert.IsType<EmbeddingVectorStore>(scopedServices.GetRequiredService<IEmbeddingVectorStore>());
            Assert.IsType<InstructionRuleRepository>(scopedServices.GetRequiredService<IInstructionRuleRepository>());
            Assert.IsType<PolicyRuleRepository>(scopedServices.GetRequiredService<IPolicyRuleRepository>());
            Assert.IsType<MemoryMetadataRepository>(scopedServices.GetRequiredService<IMemoryMetadataRepository>());
            Assert.IsType<MemorySearchStore>(scopedServices.GetRequiredService<IMemorySearchStore>());
            Assert.IsType<ContextCandidateStore>(scopedServices.GetRequiredService<IContextCandidateStore>());
            Assert.IsType<ProjectRepository>(scopedServices.GetRequiredService<IProjectRepository>());
            Assert.IsType<ProjectTaskRepository>(scopedServices.GetRequiredService<IProjectTaskRepository>());
            Assert.IsType<BindingRepository>(scopedServices.GetRequiredService<IBindingRepository>());
            Assert.IsType<OrganizationRepository>(scopedServices.GetRequiredService<IOrganizationRepository>());
            Assert.IsType<OrganizationMembershipRepository>(scopedServices
                .GetRequiredService<IOrganizationMembershipRepository>());
            Assert.IsType<WorkspaceContextSearchStore>(
                scopedServices.GetRequiredService<IWorkspaceContextSearchStore>());
        }

        [Fact]
        public void AddInfrastructure_ShouldUseDbContextAsUnitOfWork()
        {
            ServiceCollection services = new();

            services.AddInfrastructure(ConnectionString);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();

            EspadaDbContext dbContext = scope.ServiceProvider.GetRequiredService<EspadaDbContext>();

            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            Assert.Same(dbContext, unitOfWork);
        }

        [Fact]
        public void AddInfrastructure_ShouldRegisterSystemClock()
        {
            ServiceCollection services = new();

            services.AddInfrastructure(ConnectionString);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            IClockService clockService = serviceProvider.GetRequiredService<IClockService>();

            Assert.IsType<SystemClockService>(clockService);
        }

        [Fact]
        public void Repositories_ShouldBeScoped()
        {
            ServiceCollection services = new();

            services.AddInfrastructure(ConnectionString);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope firstScope = serviceProvider.CreateScope();
            using IServiceScope secondScope = serviceProvider.CreateScope();

            IArtifactRepository firstRepository = firstScope.ServiceProvider.GetRequiredService<IArtifactRepository>();

            IArtifactRepository sameScopeRepository =
                firstScope.ServiceProvider.GetRequiredService<IArtifactRepository>();

            IArtifactRepository secondRepository =
                secondScope.ServiceProvider.GetRequiredService<IArtifactRepository>();

            Assert.Same(firstRepository, sameScopeRepository);
            Assert.NotSame(firstRepository, secondRepository);
        }
    }
}