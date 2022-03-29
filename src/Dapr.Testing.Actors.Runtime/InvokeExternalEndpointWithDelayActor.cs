using Dapr.Actors.Runtime;
using Dapr.Testing.Actors.Abstractions;
using Dapr.Testing.Sdk;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.Actors.Runtime;

[Actor(TypeName = "InvokeExternalEndpointWithDelay")]
public class InvokeExternalEndpointWithDelayActor :
    BaseActor,
    IInvokeExternalEndpointWithDelayActor
{
    private const string StateName = "Invoke-External-Endpoint";
    private readonly IDelayMeHttpClient _delayMeHttpClient;
    private readonly SimpleActorState _state;

    public InvokeExternalEndpointWithDelayActor(
        ActorHost host,
        IOptions<ApplicationOptions> options,
        IDelayMeHttpClient delayMeHttpClient)
        : base(host, options)
    {
        _delayMeHttpClient = delayMeHttpClient;
        _state = new SimpleActorState();
    }

    public async Task InvokeEndpoint(int delay)
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "PodName", Options.PodName },
            { "ContainerId", Options.ContainerId }
        };

        using (Logger.BeginScope(scopeParameters))
        {
            Logger.LogInformation("Starting business logic");

            var response = await _delayMeHttpClient.Delay(delay);

            if (response.IsSuccessStatusCode)
            {
                Logger.LogInformation("Successfully invoked external endpoint");
            }
            else
            {
                Logger.LogError("Failed to invoke external endpoint");
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
        => $"Invoke-External-Endpoint-{count}-State";
}
