using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using DockerCosmosDB.Backend.Models;

namespace DockerCosmosDB.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly ILogger<NoteController> _logger;
    private readonly Container _container;

    public NoteController(ILogger<NoteController> logger, Container container)
    {
        _logger = logger;
        _container = container;
    }

    // GET: api/Note
    [HttpGet]
    public async Task<IActionResult> GetAllNotes()
    {
        try
        {
            _logger.LogInformation("Retrieving all notes from Cosmos DB");

            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<NoteModel>(query);
            
            var notes = new List<NoteModel>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                notes.AddRange(response.ToList());
            }

            _logger.LogInformation($"Retrieved {notes.Count} notes from Cosmos DB");
            
            return Ok(new
            {
                success = true,
                count = notes.Count,
                notes = notes
            });
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error retrieving notes from Cosmos DB");
            return BadRequest(new
            {
                success = false,
                message = $"Error retrieving notes: {ex.Message}",
                statusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving notes");
            return StatusCode(500, new
            {
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }

    // GET: api/Note/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNote(string id)
    {
        try
        {
            _logger.LogInformation($"Retrieving note with ID: {id}");

            var response = await _container.ReadItemAsync<NoteModel>(id, new PartitionKey(id));
            var note = response.Resource;

            _logger.LogInformation($"Successfully retrieved note with ID: {id}");

            return Ok(new
            {
                success = true,
                note = note,
                requestCharge = response.RequestCharge
            });
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"Note with ID {id} not found");
            return NotFound(new
            {
                success = false,
                message = $"Note with ID {id} not found"
            });
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, $"Error retrieving note with ID: {id}");
            return BadRequest(new
            {
                success = false,
                message = $"Error retrieving note: {ex.Message}",
                statusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error retrieving note with ID: {id}");
            return StatusCode(500, new
            {
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }

    // POST: api/Note
    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] NoteModel note)
    {
        try
        {
            if (note == null)
            {
                return BadRequest(new { success = false, message = "Note data is required" });
            }

            if (string.IsNullOrWhiteSpace(note.Title))
            {
                return BadRequest(new { success = false, message = "Note title is required" });
            }

            _logger.LogInformation("Creating new note in Cosmos DB");

            // Ensure we have a new ID and created timestamp
            note.Id = Guid.NewGuid().ToString();
            note.CreatedAt = DateTime.UtcNow;

            var response = await _container.CreateItemAsync(note, new PartitionKey(note.Id));

            _logger.LogInformation($"Successfully created note with ID: {note.Id}");

            return CreatedAtAction(
                nameof(GetNote),
                new { id = note.Id },
                new
                {
                    success = true,
                    message = "Note created successfully",
                    note = response.Resource,
                    requestCharge = response.RequestCharge
                });
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating note in Cosmos DB");
            return BadRequest(new
            {
                success = false,
                message = $"Error creating note: {ex.Message}",
                statusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating note");
            return StatusCode(500, new
            {
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }

    // PUT: api/Note/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(string id, [FromBody] NoteModel updatedNote)
    {
        try
        {
            if (updatedNote == null)
            {
                return BadRequest(new { success = false, message = "Note data is required" });
            }

            if (string.IsNullOrWhiteSpace(updatedNote.Title))
            {
                return BadRequest(new { success = false, message = "Note title is required" });
            }

            _logger.LogInformation($"Updating note with ID: {id}");

            // First, try to get the existing note
            var existingResponse = await _container.ReadItemAsync<NoteModel>(id, new PartitionKey(id));
            var existingNote = existingResponse.Resource;

            // Update the fields but keep the original ID and CreatedAt
            existingNote.Title = updatedNote.Title;
            existingNote.Content = updatedNote.Content;

            var response = await _container.ReplaceItemAsync(existingNote, id, new PartitionKey(id));

            _logger.LogInformation($"Successfully updated note with ID: {id}");

            return Ok(new
            {
                success = true,
                message = "Note updated successfully",
                note = response.Resource,
                requestCharge = response.RequestCharge
            });
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"Note with ID {id} not found for update");
            return NotFound(new
            {
                success = false,
                message = $"Note with ID {id} not found"
            });
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, $"Error updating note with ID: {id}");
            return BadRequest(new
            {
                success = false,
                message = $"Error updating note: {ex.Message}",
                statusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error updating note with ID: {id}");
            return StatusCode(500, new
            {
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }

    // DELETE: api/Note/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(string id)
    {
        try
        {
            _logger.LogInformation($"Deleting note with ID: {id}");

            var response = await _container.DeleteItemAsync<NoteModel>(id, new PartitionKey(id));

            _logger.LogInformation($"Successfully deleted note with ID: {id}");

            return Ok(new
            {
                success = true,
                message = "Note deleted successfully",
                requestCharge = response.RequestCharge
            });
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"Note with ID {id} not found for deletion");
            return NotFound(new
            {
                success = false,
                message = $"Note with ID {id} not found"
            });
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, $"Error deleting note with ID: {id}");
            return BadRequest(new
            {
                success = false,
                message = $"Error deleting note: {ex.Message}",
                statusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error deleting note with ID: {id}");
            return StatusCode(500, new
            {
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }
}
