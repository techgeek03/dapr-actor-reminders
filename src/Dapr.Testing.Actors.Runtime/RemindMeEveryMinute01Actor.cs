using Dapr.Actors.Runtime;
using Dapr.Testing.Actors.Abstractions;
using Dapr.Testing.Sdk;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.Actors.Runtime;

[Actor(TypeName = "RemindMeEveryMinute01")]
public class RemindMeEveryMinute01Actor : Actor, IRemindMeEveryMinute01Actor, IRemindable
{
    private const string GetStateName = "RemindMe-Every-Minute-State";
    private readonly ILogger<RemindMeEveryMinute01Actor> _logger;
    private readonly ApplicationOptions _options;
    private readonly RemindMeEveryMinuteState _state;

    private IDisposable? _loggerScope;

    public RemindMeEveryMinute01Actor(
        ActorHost host,
        ILogger<RemindMeEveryMinute01Actor> logger,
        IOptions<ApplicationOptions> options)
        : base(host)
    {
        _logger = logger;
        _options = options.Value;
        _state = new RemindMeEveryMinuteState();
    }

    public async Task ReceiveReminderAsync(
        string reminderName,
        byte[] state,
        TimeSpan dueTime,
        TimeSpan period)
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "ReminderName", reminderName },
            { "ReminderDueTime", dueTime },
            { "ReminderPeriod", period },
            { "PodName", _options.PodName },
            { "ContainerId", _options.ContainerId }
        };

        using (_logger.BeginScope(scopeParameters))
        {
            _logger.LogInformation("Reminder received");
            if (_state.Count == 0)
            {
                _state.StartedAt = DateTime.UtcNow;
            }

            if (_state.Count > 9)
            {
                _logger.LogInformation("Reminder count reached 10, unregistering reminder");
                await UnregisterReminderAsync($"RemindMe-Every-Minute-{Id}-Reminder");
            }
            else
            {
                _state.Count++;
                _logger.LogInformation("Reminder count: {Count}", _state.Count);
                await StateManager.AddOrUpdateStateAsync(GetStateName, _state, (key, value) => _state);

                var operationState = new RemindMeEveryMinuteOperationState
                {
                    OccuredAt = DateTime.UtcNow,
                    WasOperationAlreadyExecuted = false
                };
                var hasOperationBeenAdded = await StateManager.TryAddStateAsync(GetOperationStateName(_state.Count), operationState);

                if (hasOperationBeenAdded)
                {
                    _logger.LogCritical("Detected duplicate operation. The operation count is {Count}", _state.Count);
                    operationState.WasOperationAlreadyExecuted = true;
                    await StateManager.SetStateAsync(GetOperationStateName(_state.Count), operationState);
                }
                else
                {
                    _logger.LogInformation("Operation {Count} state added", _state.Count);
                }
            }

            await SaveStateAsync();
        }
    }

    public async Task<RemindMeEveryMinuteState> GetState()
    {
        var result = await StateManager.TryGetStateAsync<RemindMeEveryMinuteState>(GetStateName);

        if (!result.HasValue)
        {
            _logger.LogInformation("State not found. Reminder was not executed yet maybe");
        }

        return result.Value;
    }

    public async Task RegisterReminder()
    {
        await RegisterReminderAsync(
            $"RemindMe-Every-Minute-{Id}-Reminder",
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1));

        _logger.LogInformation("Actor reminder registered");
    }

    protected override Task OnActivateAsync()
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "ActorId", Id.ToString() },
            { "PodName", _options.PodName },
            { "ContainerId", _options.ContainerId }
        };

        using (_logger.BeginScope(scopeParameters))
        {
            _logger.LogInformation("Actor activated");
        }

        return base.OnActivateAsync();
    }

    protected override Task OnDeactivateAsync()
    {
        var scopeParameters = new Dictionary<string, object>
        {
            { "ActorId", Id.ToString() },
            { "PodName", _options.PodName },
            { "ContainerId", _options.ContainerId }
        };

        using (_logger.BeginScope(scopeParameters))
        {
            _logger.LogInformation("Actor deactivated");
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
            { "PodName", _options.PodName },
            { "ContainerId", _options.ContainerId }
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

    private string GetOperationStateName(int count)
        => $"RemindMe-Every-Minute-Operation-{count}-State";
}
