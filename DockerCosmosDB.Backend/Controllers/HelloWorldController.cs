using Microsoft.AspNetCore.Mvc;

namespace DockerCosmosDB.Backend.Controllers;

[ApiController]
[Route("[controller]")]

public class HelloWorldController : Controller
{
    private readonly ILogger<HelloWorldController> _logger;

    public HelloWorldController(ILogger<HelloWorldController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "HelloWorldGet")]
    public IActionResult Get()
    {
        _logger.LogInformation("HelloWorld endpoint was called.");

        var message = new { message = "helloworld" };

        return Json(message);
    }
}
