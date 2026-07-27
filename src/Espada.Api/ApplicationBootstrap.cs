using Espada.Api.Extensions;
using Espada.Application.Extensions;
using Espada.Billing;
using Espada.Infrastructure.Extensions;
using Espada.ServiceDefaults.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.ConfigureHostBuilder(builder.Configuration, builder.Environment);

builder
    .Services
    .ConfigureApi(builder.Configuration, builder.Environment)
    .ConfigureApplicationLayer()
    .ConfigureInfrastructure(builder.Configuration);
builder.Services.AddEspadaBilling(builder.Configuration);

WebApplication app = builder.Build();

app.BuildApplication();

app.Run();