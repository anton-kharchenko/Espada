using Espada.Api.Controllers;
using Espada.Application.Contracts.Persistence;
using Espada.Comms.Core.Pagination;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;

namespace Espada.Tests.Architecture;

public sealed class ArchitectureTests
{
    private static readonly Assembly CommsCoreAssembly = typeof(CursorCodec).Assembly;
    private static readonly Assembly DomainAssembly = typeof(DomainError).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IArtifactRepository).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Espada.Infrastructure.Extensions.InfrastructureServiceCollectionExtensions).Assembly;
    private static readonly Assembly ApiContractsAssembly = typeof(Api.Contracts.Responses.ErrorResponse).Assembly;
    private static readonly Assembly ApiAssembly = typeof(BaseController).Assembly;

    [Fact]
    public void ProductionProjectReferences_ShouldMatchAllowedDependencyGraph()
    {
        IReadOnlyDictionary<string, string[]> expectedReferences = new Dictionary<string, string[]>
        {
            ["Espada.Comms.Core"] = [],
            ["Espada.Domain"] = [],
            ["Espada.Application"] = ["Espada.Domain"],
            ["Espada.Infrastructure"] = ["Espada.Application", "Espada.Db", "Espada.Domain"],
            ["Espada.Db"] = ["Espada.Domain"],
            ["Espada.DeploymentKit"] = [],
            ["Espada.Deployment"] = ["Espada.DeploymentKit"],
            ["Espada.Api.Contracts"] = ["Espada.Domain"],
            ["Espada.Protocol.Mcp"] = [],
            ["Espada.Cli"] = ["Espada.Comms.Core", "Espada.Protocol.Mcp"],
            ["Espada.Daemon"] = ["Espada.Application", "Espada.Comms.Core", "Espada.Infrastructure", "Espada.Protocol.Mcp", "Espada.ServiceDefaults"],
            ["Espada.Api"] = ["Espada.Api.Contracts", "Espada.Application", "Espada.Comms.Core", "Espada.Domain", "Espada.Infrastructure", "Espada.ServiceDefaults"],
            ["Aspire"] = ["Espada.Api", "Espada.Daemon", "Espada.Db"]
        };

        string repositoryRoot = FindRepositoryRoot();

        foreach ((string projectName, string[] expected) in expectedReferences)
        {
            string projectPath = Directory.GetFiles(Path.Join(repositoryRoot, "src", projectName), "*.csproj", SearchOption.TopDirectoryOnly).Single();
            string[] actual = XDocument.Load(projectPath)
                .Descendants("ProjectReference")
                .Select(reference => Path.GetFileNameWithoutExtension(reference.Attribute("Include")!.Value))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expected.Order(StringComparer.Ordinal), actual);
        }
    }

    [Fact]
    public void Domain_ShouldNotReferenceOuterOrFrameworkLayers()
    {
        string[] forbiddenPrefixes = ["Espada.", "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "MediatR", "Npgsql", "ModelContextProtocol"];
        string[] references = DomainAssembly.GetReferencedAssemblies().Select(reference => reference.Name!).ToArray();

        Assert.DoesNotContain(references, reference => forbiddenPrefixes.Any(prefix => reference.StartsWith(prefix, StringComparison.Ordinal)));
    }

    [Fact]
    public void ApplicationHandlers_ShouldUseHandlerSuffix()
    {
        Type[] handlers = ApplicationAssembly.GetTypes()
            .Where(type => !type.IsAbstract && type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToArray();

        Assert.NotEmpty(handlers);
        Assert.All(handlers, handler => Assert.EndsWith("Handler", handler.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Controllers_ShouldStayInsideApiControllerBoundary()
    {
        Type[] controllers = ApiAssembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(controllers);
        Assert.All(controllers, controller =>
        {
            Assert.Equal("Espada.Api.Controllers", controller.Namespace);
            Assert.EndsWith("Controller", controller.Name, StringComparison.Ordinal);
            Assert.True(controller.IsSealed, $"{controller.FullName} must be sealed.");
        });
    }

    [Fact]
    public void PublicApiContracts_ShouldStayInsideContractsNamespace()
    {
        Type[] publicContracts = ApiContractsAssembly.GetExportedTypes();

        Assert.NotEmpty(publicContracts);
        Assert.All(publicContracts, contract => Assert.StartsWith("Espada.Api.Contracts.", contract.Namespace, StringComparison.Ordinal));
    }

    [Fact]
    public void LayerNamespaces_ShouldMatchOwningAssembly()
    {
        AssertNamespaces(CommsCoreAssembly, "Espada.Comms.Core");
        AssertNamespaces(DomainAssembly, "Espada.Domain");
        AssertNamespaces(ApplicationAssembly, "Espada.Application");
        AssertNamespaces(InfrastructureAssembly, "Espada.Infrastructure");
    }

    private static void AssertNamespaces(Assembly assembly, string expectedPrefix)
    {
        Type[] types = assembly.GetTypes().Where(type => type.Namespace is not null).ToArray();
        Assert.All(types, type => Assert.StartsWith(expectedPrefix, type.Namespace, StringComparison.Ordinal));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Join(directory.FullName, "Espada.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Espada repository root was not found.");
    }
}