using Newtonsoft.Json;

namespace DockerCosmosDB.Backend.Models;

public class NoteModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;
    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}