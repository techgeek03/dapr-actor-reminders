using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace Dapr.Testing.Sdk;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapHealthzChecks(
        this IEndpointRouteBuilder endpoints)
    {
        var readinessOptions = new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        };

        var livenessOptions = new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        };

        endpoints
            .MapHealthChecks("/healthz/ready", readinessOptions)
            .RequireHost("*:8081");

        endpoints
            .MapHealthChecks("/healthz/live", livenessOptions)
            .RequireHost("*:8081");

        return endpoints;
    }
}
