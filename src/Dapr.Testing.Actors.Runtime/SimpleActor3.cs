using Dapr.Actors.Runtime;
using Dapr.Testing.Actors.Abstractions;
using Dapr.Testing.Sdk;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.Actors.Runtime;

[Actor(TypeName = "SimpleActor3")]
public class SimpleActor3 :
    BaseActor,
    ISimpleActor
{
    private const string StateName = "Simple-Actor-State";
    private readonly SimpleActorState _state;

    public SimpleActor3(
        ActorHost host,
        IOptions<ApplicationOptions> options)
        : base(host, options)
    {
        _state = new SimpleActorState();
    }

    public async Task DoSomething(TimeSpan delay)
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "PodName", Options.PodName },
            { "ContainerId", Options.ContainerId }
        };

        using (Logger.BeginScope(scopeParameters))
        {
            Logger.LogInformation("Starting business logic");

            for (int i = 0; i < delay.TotalSeconds; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine($"Executing business logic after {i} s");
            }

            _state.Count++;
            _state.Timestamp = DateTimeOffset.UtcNow;
            Logger.LogInformation("Simple actor count: {Count}", _state.Count);
            await StateManager.AddOrUpdateStateAsync(StateName, _state, (key, value) => _state);

            var operationState = new SimpleActorOperationState
            {
                OccuredAt = DateTime.UtcNow, WasOperationAlreadyExecuted = false, OccuredAtPodName = Options.PodName
            };
            var wasStateAdded = await StateManager.TryAddStateAsync(GetOperationStateName(_state.Count), operationState);

            if (wasStateAdded)
            {
                Logger.LogInformation("Operation {Count} state added", _state.Count);
            }
            else
            {
                Logger.LogCritical("Detected duplicate operation. The operation count is {Count}", _state.Count);
                operationState.WasOperationAlreadyExecuted = true;
                await StateManager.SetStateAsync(GetOperationStateName(_state.Count), operationState);
            }

            await SaveStateAsync();
            Logger.LogInformation("Finished business logic");
        }
    }

    protected override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();

        var result = await StateManager.TryGetStateAsync<SimpleActorState>(StateName);

        if (result.HasValue)
        {
            _state.Count = result.Value.Count;
            _state.StartedAt = result.Value.StartedAt;
            _state.Timestamp = result.Value.Timestamp;
        }
    }

    private string GetOperationStateName(int count)
        => $"Simple-Actor-Operation-{count}-State";
}
