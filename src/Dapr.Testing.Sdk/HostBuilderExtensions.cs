using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace Dapr.Testing.Sdk;

public static class HostBuilderExtensions
{
    private const ActivityTrackingOptions TraceAndLogsCorrelationOptions =
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.SpanId;

    public static void ConfigureLogging(ILoggingBuilder builder)
        => builder.Configure(options => options.ActivityTrackingOptions = TraceAndLogsCorrelationOptions);

    public static IHostBuilder UseHost(this IHostBuilder hostBuilder)
        => hostBuilder
            .UseSerilog((context, loggerConfiguration) =>
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .Enrich.With<DiagnosticsActivityLogEnricher>()
                    .Filter.ByExcluding(FilterLogs)
                    .WriteTo.Console(new RenderedCompactJsonFormatter()));

    private static bool FilterLogs(LogEvent c)
        => c.Properties.Any(p => p.Value.ToString().Contains("/healthz/", StringComparison.InvariantCultureIgnoreCase));
}
