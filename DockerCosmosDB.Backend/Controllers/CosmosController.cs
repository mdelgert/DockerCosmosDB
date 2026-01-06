using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace DockerCosmosDB.Backend.Controllers;

public class TestRecord
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

[ApiController]
[Route("api/[controller]")]
public class CosmosController : ControllerBase
{
    private readonly ILogger<CosmosController> _logger;
    private readonly Container _container;

    public CosmosController(ILogger<CosmosController> logger, Container container)
    {
        _logger = logger;
        _container = container;
    }

    [HttpPost("add-test-record")]
    public async Task<IActionResult> AddTestRecord()
    {
        try
        {
            _logger.LogInformation("Adding test record to Cosmos DB");
            
            var testRecord = new TestRecord
            {
                Name = "Test Record",
                Description = "This is a test record added to Cosmos DB"
            };

            var response = await _container.CreateItemAsync(
                testRecord, 
                new PartitionKey(testRecord.Id)
            );

            _logger.LogInformation($"Test record created with ID: {testRecord.Id}");
            
            return Ok(new 
            { 
                success = true,
                id = testRecord.Id,
                message = "Test record added successfully",
                record = testRecord,
                requestCharge = response.RequestCharge
            });
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating test record in Cosmos DB");
            return BadRequest(new 
            { 
                success = false,
                message = $"Error creating record: {ex.Message}",
                statusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating test record");
            return StatusCode(500, new 
            { 
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }
}
