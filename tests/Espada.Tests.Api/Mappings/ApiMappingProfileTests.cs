using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Workspaces;
using Espada.Api.Mappings;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Api.Mappings;

public sealed class ApiMappingProfileTests
{
    [Fact]
    public void Profile_ShouldBeValid()
    {
        MapperConfiguration configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_CreateWorkspaceSource_ShouldMapRequestAndIdentity()
    {
        MapperConfiguration configuration = CreateConfiguration();
        IMapper mapper = configuration.CreateMapper();
        CreateWorkspaceMappingSource source = new(
            new CreateWorkspaceRequest
            {
                Name = "Mapped workspace",
                TypeId = WorkspaceType.Personal.Id
            },
            "issuer",
            "subject");

        CreateWorkspaceCommand command = mapper.Map<CreateWorkspaceCommand>(source);

        Assert.Equal(source.Request.Name, command.Name);
        Assert.Equal(WorkspaceType.Personal, command.Type);
        Assert.Equal(source.IdentityIssuer, command.IdentityIssuer);
        Assert.Equal(source.IdentitySubject, command.IdentitySubject);
    }

    private static MapperConfiguration CreateConfiguration() =>
        new(options => options.AddProfile<ApiMappingProfile>(), NullLoggerFactory.Instance);
}