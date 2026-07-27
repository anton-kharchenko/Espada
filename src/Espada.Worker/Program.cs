using Espada.Application.Extensions;
using Espada.Infrastructure.Extensions;
using Espada.ServiceDefaults;
using Espada.Worker;
using Espada.Billing;
using Espada.ServiceDefaults.Extensions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services
    .ConfigureApplicationLayer()
    .ConfigureInfrastructure(builder.Configuration);
builder.Services.AddEspadaBilling(builder.Configuration);
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
await host.RunAsync();