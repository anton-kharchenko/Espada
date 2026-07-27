using Espada.ServiceDefaults.Extensions;
using Scalar.AspNetCore;

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
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapDefaultEndpoints();

        return app;
    }
}