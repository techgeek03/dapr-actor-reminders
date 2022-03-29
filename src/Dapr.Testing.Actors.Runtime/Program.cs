using Dapr.Testing.Actors.Runtime;
using Dapr.Testing.Sdk;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostOptions(o => o.ShutdownTimeout = TimeSpan.FromSeconds(60));

Log.Logger = LoggingProvider.CreateSerilogLogger();

// Configure Logging
builder.Logging.ConfigureLogging();

builder.Host.UseLogging();

builder.Services.AddOptions();

builder.Services.AddHealthChecks();

builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));

builder.Services.AddHttpClient(nameof(SimpleActor2), client =>
{
    var httpsHttpbinOrgDelay = "https://httpbin.org/delay/10";
    client.BaseAddress = new Uri(httpsHttpbinOrgDelay);
});

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<RemindMeEveryMinute01Actor>();
    options.Actors.RegisterActor<RemindMeEveryMinute02Actor>();
    options.Actors.RegisterActor<SimpleActor1>();
    options.Actors.RegisterActor<SimpleActor2>();
    options.Actors.RegisterActor<SimpleActor3>();

    options.ActorIdleTimeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Dapr:Actors:IdleTimeoutSeconds"));
    options.ActorScanInterval = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Dapr:Actors:ScanIntervalSeconds"));
    options.DrainRebalancedActors = true;
    options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthzChecks();
    endpoints.MapActorsHandlers();
});

app.Run();
