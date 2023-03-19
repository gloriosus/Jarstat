using System.Text.Json.Serialization;

namespace Jarstat.Client.Requests;

public class UpdateUserRequest
{
    [JsonPropertyName("id")]
    public Guid UserId { get; set; }
    public string Password { get; set; } = null!;
    public Guid LastUpdaterId { get; set; }
}
