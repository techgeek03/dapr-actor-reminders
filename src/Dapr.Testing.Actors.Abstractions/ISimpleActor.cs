using Dapr.Actors;

namespace Dapr.Testing.Actors.Abstractions;

public interface ISimpleActor : IActor
{
    Task DoSomething(TimeSpan delay);
}

public interface IInvokeExternalEndpointWithDelayActor : IActor
{
    Task InvokeEndpoint(int delay);
}

public class SimpleActorState
{
    public int Count { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
}

public class SimpleActorOperationState
{
    public DateTime OccuredAt { get; set; }

    public bool WasOperationAlreadyExecuted { get; set; }

    public string OccuredAtPodName { get; set; } = default!;
}
