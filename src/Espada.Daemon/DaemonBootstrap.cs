using Espada.Daemon.Extensions;
using Espada.ServiceDefaults.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string dataRoot = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
    ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada");
builder.Configuration.AddJsonFile(Path.Join(dataRoot, "runtime.json"), optional: true, reloadOnChange: false);

if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
{
    builder.WebHost.UseUrls("http://127.0.0.1:7431");
}

builder.AddServiceDefaults();
builder.Services.AddEspadaDaemon(builder.Configuration);

WebApplication app = builder.Build();
app.UseEspadaDaemon();
app.MapDefaultEndpoints();

app.Run();