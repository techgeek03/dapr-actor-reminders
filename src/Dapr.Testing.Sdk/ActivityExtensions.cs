using System.Diagnostics;

namespace Dapr.Testing.Sdk;

internal static class ActivityExtensions
{
    public static string GetSpanId(this Activity activity)
    {
        var spanId = activity.IdFormat switch
        {
            ActivityIdFormat.Hierarchical => activity.Id,
            ActivityIdFormat.W3C => activity.SpanId.ToHexString(),
            _ => null
        };

        return spanId ?? string.Empty;
    }

    public static string GetTraceId(this Activity activity)
    {
        var traceId = activity.IdFormat switch
        {
            ActivityIdFormat.Hierarchical => activity.RootId,
            ActivityIdFormat.W3C => activity.TraceId.ToHexString(),
            _ => null
        };

        return traceId ?? string.Empty;
    }

    public static string GetParentId(this Activity activity)
    {
        var parentId = activity.IdFormat switch
        {
            ActivityIdFormat.Hierarchical => activity.ParentId,
            ActivityIdFormat.W3C => activity.ParentSpanId.ToHexString(),
            _ => null
        };

        return parentId ?? string.Empty;
    }
}
