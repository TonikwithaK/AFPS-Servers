using AFPS_Servers.Infrastructure.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration sources: appsettings.json, user secrets, env vars
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddAfpsServices(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
