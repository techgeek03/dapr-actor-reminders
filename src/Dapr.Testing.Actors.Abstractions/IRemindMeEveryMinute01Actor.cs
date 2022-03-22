using Dapr.Actors;

namespace Dapr.Testing.Actors.Abstractions;

public interface IRemindMeEveryMinute01Actor : IActor
{
    Task<RemindMeEveryMinuteState> GetState();

    Task RegisterReminder();
}

public interface IRemindMeEveryMinute02Actor : IActor
{
    Task RegisterReminder();
}

public class RemindMeEveryMinuteState
{
    public int Count { get; set; }

    public DateTime StartedAt { get; set; }
}

public class RemindMeEveryMinuteOperationState
{
    public DateTime OccuredAt { get; set; }

    public bool WasOperationAlreadyExecuted { get; set; }
}
