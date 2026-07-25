using Espada.ServiceDefaults;

namespace Espada.Api.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static WebApplication BuildApplication(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.MapControllers();
        app.MapDefaultEndpoints();

        return app;
    }
}