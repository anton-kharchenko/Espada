using Espada.Application.Contracts.Security;
using Espada.Application.Extensions;
using Espada.Billing.Extensions;
using Espada.Infrastructure.Extensions;
using Espada.ServiceDefaults.Extensions;
using Espada.Worker;
using Espada.Worker.Repositories;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services
    .ConfigureApplicationLayer()
    .ConfigureInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IRequestPrincipalAccessor,
    BackgroundRequestPrincipalAccessor>();
builder.Services.AddEspadaBilling(builder.Configuration);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<RepositoryWatcherService>();

IHost host = builder.Build();
await host.RunAsync();