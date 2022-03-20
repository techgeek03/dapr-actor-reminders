using System.Diagnostics;
using Dapr.Testing.Sdk;
using Serilog;

namespace Dapr.Testing.Actors.Runtime;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        Log.Logger = LoggingProvider.CreateSerilogLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Log.Fatal(ex, "Host terminated unexpectedly");
            Thread.Sleep(5000);
            return -1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .ConfigureLogging(HostBuilderExtensions.ConfigureLogging)
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
        .UseHost();
}
