using Microsoft.AspNetCore.Mvc;

namespace Dapr.Testing.WebApi.Controllers;

[ApiController]
[Route("/api/actor-reminders-tests")]
public class ActorTestsController : ControllerBase
{
    private readonly ILogger<ActorTestsController> _logger;

    public ActorTestsController(
        ILogger<ActorTestsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("test01")]
    public IActionResult Test01()
    {
        _logger.LogDebug("Start test 01");
        return Ok(new { Name = "Test 01" });
    }
}
