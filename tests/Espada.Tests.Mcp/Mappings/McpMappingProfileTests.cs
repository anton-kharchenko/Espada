using AutoMapper;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Mcp.Mappings
{
    public sealed class McpMappingProfileTests
    {
        [Fact]
        public void Configuration_ShouldBeValid()
        {
            MapperConfiguration configuration = CreateConfiguration();

            configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public void ArtifactCreate_ShouldDisablePolicyMutation()
        {
            IMapper mapper = CreateConfiguration().CreateMapper();
            ArtifactCreateRequest request = new(
                Guid.NewGuid(),
                "Document",
                1,
                "Content");

            CreateArtifactCommand command =
                mapper.Map<CreateArtifactCommand>(request);

            Assert.False(command.AllowPolicyMutation);
        }

        [Fact]
        public void ArtifactRevise_ShouldDisablePolicyMutation()
        {
            IMapper mapper = CreateConfiguration().CreateMapper();
            ArtifactReviseRequest request = new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Revised content");

            AddArtifactRevisionCommand command =
                mapper.Map<AddArtifactRevisionCommand>(request);

            Assert.False(command.AllowPolicyMutation);
            Assert.Null(command.RequiredKindTypeId);
        }

        private static MapperConfiguration CreateConfiguration()
        {
            return new MapperConfiguration(
                options => options.AddProfile<McpMappingProfile>(),
                NullLoggerFactory.Instance);
        }
    }
}
