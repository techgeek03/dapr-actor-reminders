using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Testing.WebApi.Controllers;

[ApiController]
[Route("/api/actor-tests")]
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
    public async Task<IActionResult> Test01(string? id = null, int secondsDelay = 1, CancellationToken cancellationToken = default)
    {
        var actorId = new ActorId(id ?? Guid.NewGuid().ToString());
        var proxy = _actorProxy.Create(actorId, "SimpleActor");

        await proxy.InvokeMethodAsync("DoSomething", TimeSpan.FromSeconds(secondsDelay), cancellationToken);
        return Accepted();
    }
}
