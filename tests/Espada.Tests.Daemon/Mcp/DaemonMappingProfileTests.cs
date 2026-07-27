using AutoMapper;
using Espada.Daemon.Mappings;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Espada.Tests.Daemon.Mcp;

public sealed class DaemonMappingProfileTests
{
    [Fact]
    public void Profile_ShouldBeValid()
    {
        MapperConfiguration configuration = new(options => options.AddProfile<DaemonMappingProfile>(), NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}