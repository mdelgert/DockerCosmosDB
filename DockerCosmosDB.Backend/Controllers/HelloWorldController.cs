using Microsoft.AspNetCore.Mvc;

namespace DockerCosmosDB.Backend.Controllers;

[ApiController]
[Route("[controller]")]

public class HelloWorldController : Controller
{
    private readonly ILogger<WeatherForecastController> _logger;

    public HelloWorldController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "HelloWorldGet")]
    public IActionResult Get()
    {
        var message = new { message = "helloworld" };

        return Json(message);
    }
}
