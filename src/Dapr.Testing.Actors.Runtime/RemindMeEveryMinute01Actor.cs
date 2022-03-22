using Dapr.Actors.Runtime;
using Dapr.Testing.Actors.Abstractions;
using Dapr.Testing.Sdk;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.Actors.Runtime;

[Actor(TypeName = "RemindMeEveryMinute01")]
public class RemindMeEveryMinute01Actor :
    BaseActor,
    IRemindMeEveryMinute01Actor,
    IRemindable
{
    private const string GetStateName = "RemindMe-Every-Minute-State";
    private readonly RemindMeEveryMinuteState _state;

    public RemindMeEveryMinute01Actor(
        ActorHost host,
        IOptions<ApplicationOptions> options)
        : base(host, options)
    {
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
            { "PodName", Options.PodName },
            { "ContainerId", Options.ContainerId }
        };

        using (Logger.BeginScope(scopeParameters))
        {
            Logger.LogInformation("Reminder received");
            if (_state.Count == 0)
            {
                _state.StartedAt = DateTime.UtcNow;
            }

            if (_state.Count > 9)
            {
                Logger.LogInformation("Reminder count reached 10, unregistering reminder");
                await UnregisterReminderAsync(GetReminderName());
            }
            else
            {
                _state.Count++;
                Logger.LogInformation("Reminder count: {Count}", _state.Count);
                await StateManager.AddOrUpdateStateAsync(GetStateName, _state, (key, value) => _state);

                var operationState = new RemindMeEveryMinuteOperationState
                {
                    OccuredAt = DateTime.UtcNow,
                    WasOperationAlreadyExecuted = false
                };
                var hasOperationBeenAdded = await StateManager.TryAddStateAsync(GetOperationStateName(_state.Count), operationState);

                if (hasOperationBeenAdded)
                {
                    Logger.LogCritical("Detected duplicate operation. The operation count is {Count}", _state.Count);
                    operationState.WasOperationAlreadyExecuted = true;
                    await StateManager.SetStateAsync(GetOperationStateName(_state.Count), operationState);
                }
                else
                {
                    Logger.LogInformation("Operation {Count} state added", _state.Count);
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
            Logger.LogInformation("State not found. Reminder was not executed yet maybe");
        }

        return result.Value;
    }

    public async Task RegisterReminder()
    {
        await RegisterReminderAsync(
            GetReminderName(),
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1));

        Logger.LogInformation("Actor reminder registered");
    }

    private string GetReminderName()
        => $"RemindMe-Every-Minute-01-{Id}-Reminder";

    private string GetOperationStateName(int count)
        => $"RemindMe-Every-Minute-Operation-{count}-State";
}
