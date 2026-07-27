using Espada.Daemon.Extensions;
using Espada.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
{
    builder.WebHost.UseUrls("http://127.0.0.1:7432");
}

builder.AddServiceDefaults();
builder.Services.AddEspadaDaemon(builder.Configuration);

WebApplication app = builder.Build();
app.UseEspadaDaemon();
app.MapDefaultEndpoints();

app.Run();

public partial class Program;