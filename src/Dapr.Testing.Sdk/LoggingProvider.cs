using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Dapr.Testing.Sdk;

public static class LoggingProvider
{
    public static ILogger CreateSerilogLogger()
        => new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(new RenderedCompactJsonFormatter())
            .CreateLogger();
}
