using Dapr.Testing.Sdk;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dapr.Testing.WebApi.Controllers;

[ApiController]
[Route("")]
public class RootController : ControllerBase
{
    private readonly ILogger<RootController> _logger;
    private readonly ApplicationOptions _options;

    public RootController(
        ILogger<RootController> logger,
        IOptions<ApplicationOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    [HttpGet(Name = "GetRootEndpoint")]
    public IActionResult Get()
    {
        _logger.LogDebug("Root controller invoked");
        return Ok(new
        {
            Name = "Dapr Testing API",
            _options.PodName,
            _options.ContainerId
        });
    }
}
