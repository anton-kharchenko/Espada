using Espada.Api.Extensions;
using Espada.Application.Extensions;
using Espada.Infrastructure.Extensions;
using Espada.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.ConfigureHostBuilder(builder.Configuration, builder.Environment);

builder
    .Services
    .ConfigureApi(builder.Configuration, builder.Environment)
    .ConfigureApplicationLayer()
    .ConfigureInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.BuildApplication();

app.Run();

public partial class Program;