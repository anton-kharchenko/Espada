using Espada.Comms.Core.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Comms.Core.Security
{
    public static class ApiKeyAuthenticationExtensions
    {
        public static void AddEspadaApiKeyAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services
                .AddAuthentication(ApiKeyAuthenticationConstants.AuthenticationScheme)
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                    ApiKeyAuthenticationConstants.AuthenticationScheme,
                    options =>
                    {
                        IConfigurationSection section =
                            configuration.GetSection(ApiKeyAuthenticationConstants.ConfigurationSection);
                        options.HeaderName = section["HeaderName"] ?? ApiKeyAuthenticationConstants.DefaultHeaderName;
                        options.ApiKey =
                            Environment.GetEnvironmentVariable(ApiKeyAuthenticationConstants.EnvironmentVariable) ??
                            section["Value"] ?? string.Empty;
                    });
        }
    }
}