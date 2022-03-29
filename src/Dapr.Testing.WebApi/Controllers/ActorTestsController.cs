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
    public async Task<IActionResult> Test01(string id, int secondsDelay, CancellationToken cancellationToken = default)
    {
        var actorId = new ActorId(id);
        var proxy = _actorProxy.Create(actorId, "SimpleActor1");

        await proxy.InvokeMethodAsync("DoSomething", TimeSpan.FromSeconds(secondsDelay), cancellationToken);
        return Accepted();
    }

    [HttpGet]
    [Route("test02")]
    public async Task<IActionResult> Test02(string id, CancellationToken cancellationToken = default)
    {
        var actorId = new ActorId(id);
        var proxy = _actorProxy.Create(actorId, "SimpleActor2");

        await proxy.InvokeMethodAsync("DoSomething", TimeSpan.Zero, cancellationToken);
        return Accepted();
    }

    [HttpGet]
    [Route("test03")]
    public async Task<IActionResult> Test03(string id, int secondsDelay, CancellationToken cancellationToken = default)
    {
        var actorId = new ActorId(id);
        var proxy = _actorProxy.Create(actorId, "SimpleActor3");

        await proxy.InvokeMethodAsync("DoSomething", TimeSpan.FromSeconds(secondsDelay), cancellationToken);
        return Accepted();
    }
}
