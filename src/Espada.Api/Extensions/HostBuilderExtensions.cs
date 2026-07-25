namespace Espada.Api.Extensions;

internal static class HostBuilderExtensions
{
    public static void ConfigureHostBuilder(this IHostBuilder host, IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = environment.IsDevelopment();
            options.ValidateOnBuild = environment.IsDevelopment();
        });
    }
}