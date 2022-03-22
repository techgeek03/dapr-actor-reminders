using Dapr.Actors.Runtime;
using Dapr.Testing.Actors.Abstractions;
using Dapr.Testing.Sdk;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.Actors.Runtime;

[Actor(TypeName = "RemindMeEveryMinute02")]
public class RemindMeEveryMinute02Actor :
    BaseActor,
    IRemindMeEveryMinute02Actor,
    IRemindable
{
    public RemindMeEveryMinute02Actor(
        ActorHost host,
        IOptions<ApplicationOptions> options)
        : base(host, options)
    {
    }

    public async Task RegisterReminder()
    {
        await RegisterReminderAsync(
            $"RemindMe-Every-Minute02-{Id}-Reminder",
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1));

        Logger.LogInformation("Actor reminder registered");
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

            await SaveStateAsync();
        }
    }
}
