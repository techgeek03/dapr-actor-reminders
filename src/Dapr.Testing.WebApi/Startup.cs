using Dapr.Actors.Client;
using Dapr.Testing.Sdk;

namespace Dapr.Testing.WebApi;

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

        services.AddSingleton(ActorProxy.DefaultProxyFactory);

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthzChecks();
            endpoints.MapControllers();
        });
    }
}
