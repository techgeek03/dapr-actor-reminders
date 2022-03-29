using Dapr.Actors.Client;
using Dapr.Testing.Sdk;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = LoggingProvider.CreateSerilogLogger();

// Configure Logging
builder.Logging.ConfigureLogging();

builder.Host.UseLogging();

builder.Services.AddOptions();

builder.Services.AddHealthChecks();

builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));

builder.Services.AddSingleton(ActorProxy.DefaultProxyFactory);

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthzChecks();
    endpoints.MapControllers();
});

app.Run();
