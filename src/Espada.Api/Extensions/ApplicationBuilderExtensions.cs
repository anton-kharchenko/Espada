using Espada.Api.Authentication;
using Espada.ServiceDefaults.Extensions;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace Espada.Api.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        public static WebApplication BuildApplication(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseForwardedHeaders();
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            WebConsoleOptions webConsoleOptions = app.Services
                .GetRequiredService<IOptions<WebConsoleOptions>>()
                .Value;
            if (webConsoleOptions.Mode == WebConsoleMode.Local)
            {
                app.UseWhen(
                    context => !context.Request.Path
                        .StartsWithSegments("/bff"),
                    branch => branch.UseHttpsRedirection());
            }
            else
            {
                app.UseHttpsRedirection();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapWebConsoleEndpoints();
            app.MapLocalSyncEndpoints();

            if (!app.Environment.IsProduction())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.MapDefaultEndpoints();
            app.MapFallbackToFile("index.html");

            return app;
        }
    }
}