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
    [Route("test03")]
    public async Task<IActionResult> Test03(string id, int secondsDelay, CancellationToken cancellationToken = default)
    {
        var actorId = new ActorId(id);
        var proxy = _actorProxy.Create(actorId, "SimpleActor3");

        await proxy.InvokeMethodAsync("DoSomething", TimeSpan.FromSeconds(secondsDelay), cancellationToken);
        return Accepted();
    }

    [HttpPost("{id}")]
    [Route("invoke-external-endpoint/{id}")]
    public async Task<IActionResult> InvokeExternalEndpoint(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start invoking actor");
        var actorId = new ActorId(id);
        var proxy = _actorProxy.Create(actorId, "InvokeExternalEndpointWithDelay");

        await proxy.InvokeMethodAsync("InvokeEndpoint", 30000, cancellationToken);
        return Created(new Uri($"http://localhost:8080/api/actor-tests/invoke-external-endpoint/{id}"), null);
    }
}
