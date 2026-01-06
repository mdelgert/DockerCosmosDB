using Microsoft.AspNetCore.Mvc;

namespace DockerCosmosDB.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CosmosController : Controller
{
    private readonly ILogger<CosmosController> _logger;

    public CosmosController(ILogger<CosmosController> logger)
    {
        _logger = logger;
    }


    [HttpGet(Name = "Get")]

    public IActionResult Get()
    {
        _logger.LogInformation("CosmosController Get endpoint was called.");

        var message = new { message = "helloworld CosmosController Get" };

        return Json(message);
    }
}
