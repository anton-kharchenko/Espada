using AutoMapper;
using Espada.Application.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Mappings
{
    public sealed class ApplicationMappingProfileTests
    {
        [Fact]
        public void Profile_ShouldBeValid()
        {
            MapperConfiguration configuration = new(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();
        }
    }
}