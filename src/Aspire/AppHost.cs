using Aspire.Hosting.Espada;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddEspadaInfrastructure();

builder.Build().Run();