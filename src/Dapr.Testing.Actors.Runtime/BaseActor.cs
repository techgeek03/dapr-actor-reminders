using Dapr.Actors.Runtime;
using Dapr.Testing.Sdk;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.Actors.Runtime;

public abstract class BaseActor : Actor
{
    private IDisposable? _loggerScope;

    protected BaseActor(
        ActorHost host,
        IOptions<ApplicationOptions> options)
        : base(host)
    {
        Options = options.Value;
    }

    protected ApplicationOptions Options { get; }

    protected override Task OnActivateAsync()
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "ActorId", Id.ToString() },
            { "PodName", Options.PodName },
            { "ContainerId", Options.ContainerId }
        };

        using (Logger.BeginScope(scopeParameters))
        {
            Logger.LogInformation("Actor activated");
        }

        return base.OnActivateAsync();
    }

    protected override Task OnDeactivateAsync()
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "ActorId", Id.ToString() },
            { "PodName", Options.PodName },
            { "ContainerId", Options.ContainerId }
        };

        using (Logger.BeginScope(scopeParameters))
        {
            Logger.LogInformation("Actor deactivated");
        }

        return base.OnDeactivateAsync();
    }

    protected override Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "ActorId", Id.ToString() },
            { "MethodName", actorMethodContext.MethodName },
            { "CallType", actorMethodContext.CallType.ToString() },
            { "PodName", Options.PodName },
            { "ContainerId", Options.ContainerId }
        };

        _loggerScope = Logger.BeginScope(scopeParameters);

        return base.OnPreActorMethodAsync(actorMethodContext);
    }

    protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        var result = base.OnPostActorMethodAsync(actorMethodContext);

        _loggerScope?.Dispose();

        return result;
    }
}
