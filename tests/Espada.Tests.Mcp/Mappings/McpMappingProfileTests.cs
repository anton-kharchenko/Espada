using AutoMapper;
using Espada.Protocol.Mcp.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Mcp.Mappings;

public sealed class McpMappingProfileTests
{
    [Fact]
    public void Configuration_ShouldBeValid()
    {
        MapperConfiguration configuration = new(
            options => options.AddProfile<McpMappingProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
