using Espada.Application.Extensions;
using Espada.Billing;
using Espada.Infrastructure.Extensions;
using Espada.ServiceDefaults.Extensions;
using Espada.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services
    .ConfigureApplicationLayer()
    .ConfigureInfrastructure(builder.Configuration);
builder.Services.AddEspadaBilling(builder.Configuration);
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
await host.RunAsync();