using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Testing.Actors.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Testing.WebApi.Controllers;

[ApiController]
[Route("/api/actor-reminders-tests")]
public class ActorTestsController : ControllerBase
{
    private readonly IActorProxyFactory _actorProxy;
    private readonly ILogger<ActorTestsController> _logger;

    public ActorTestsController(
        IActorProxyFactory actorProxy,
        ILogger<ActorTestsController> logger)
    {
        _actorProxy = actorProxy;
        _logger = logger;
    }

    [HttpGet]
    [Route("test01")]
    public async Task<IActionResult> Test01()
    {
        var actorId = new ActorId(Guid.NewGuid().ToString());
        var proxy = _actorProxy.CreateActorProxy<IRemindMeEveryMinute01Actor>(actorId, "RemindMeEveryMinute01");

        await proxy.RegisterReminder();
        return Accepted();
    }
}
