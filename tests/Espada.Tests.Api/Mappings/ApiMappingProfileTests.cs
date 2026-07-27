using AutoMapper;
using Espada.Api.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Api.Mappings;

public sealed class ApiMappingProfileTests
{
    [Fact]
    public void Profile_ShouldBeValid()
    {
        MapperConfiguration configuration = new(options => options.AddProfile<ApiMappingProfile>(), NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}