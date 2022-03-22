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

        services.AddActors(options =>
        {
            options.Actors.RegisterActor<RemindMeEveryMinute01Actor>();
            options.Actors.RegisterActor<RemindMeEveryMinute02Actor>();

            options.ActorIdleTimeout = TimeSpan.FromMinutes(60);
            options.ActorScanInterval = TimeSpan.FromSeconds(30);
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
