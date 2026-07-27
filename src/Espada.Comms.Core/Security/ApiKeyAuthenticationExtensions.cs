using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Comms.Core.Security;

public static class ApiKeyAuthenticationExtensions
{
    public static void AddEspadaApiKeyAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    IConfigurationSection section = configuration.GetSection(ApiKeyAuthenticationDefaults.ConfigurationSection);
                    options.HeaderName = section["HeaderName"] ?? ApiKeyAuthenticationDefaults.DefaultHeaderName;
                    options.ApiKey = Environment.GetEnvironmentVariable(ApiKeyAuthenticationDefaults.EnvironmentVariable) ?? section["Value"] ?? string.Empty;
                });
    }
}