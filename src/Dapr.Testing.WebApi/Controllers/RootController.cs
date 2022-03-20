using Microsoft.AspNetCore.Mvc;

namespace Dapr.Testing.WebApi.Controllers;

[ApiController]
[Route("")]
public class RootController : ControllerBase
{
    private readonly ILogger<RootController> _logger;

    public RootController(
        ILogger<RootController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetRootEndpoint")]
    public IActionResult Get()
    {
        _logger.LogDebug("Root controller invoked");
        return Ok(new { Name = "Dapr Testing API" });
    }
}
