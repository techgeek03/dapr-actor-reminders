using Dapr.Testing.Sdk;

namespace Dapr.Testing.Actors.Runtime;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();
        services.AddHealthChecks();
        services.Configure<ApplicationOptions>(Configuration.GetSection("Application"));

        services.AddHttpClient(nameof(SimpleActor2), client =>
        {
            var httpsHttpbinOrgDelay = "https://httpbin.org/delay/10";
            client.BaseAddress = new Uri(httpsHttpbinOrgDelay);
        });

        services.AddActors(options =>
        {
            options.Actors.RegisterActor<RemindMeEveryMinute01Actor>();
            options.Actors.RegisterActor<RemindMeEveryMinute02Actor>();
            options.Actors.RegisterActor<SimpleActor1>();
            options.Actors.RegisterActor<SimpleActor2>();

            options.ActorIdleTimeout = TimeSpan.FromSeconds(Configuration.GetValue<int>("Dapr:Actors:IdleTimeoutSeconds"));
            options.ActorScanInterval = TimeSpan.FromSeconds(Configuration.GetValue<int>("Dapr:Actors:ScanIntervalSeconds"));
            options.DrainRebalancedActors = true;
            options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(30);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthzChecks();
            endpoints.MapActorsHandlers();
        });
    }
}
