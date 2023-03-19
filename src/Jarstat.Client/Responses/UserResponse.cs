using System.Text.Json.Serialization;

namespace Jarstat.Client.Responses;

public record UserResponse
{
    [JsonConstructor]
    public UserResponse(
        Guid userId, 
        string username, 
        DateTime dateTimeCreated, 
        DateTime dateTimeUpdated, 
        Guid? creatorId, 
        Guid? lastUpdaterId)
    {
        UserId = userId;
        UserName = username;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        CreatorId = creatorId;
        LastUpdaterId = lastUpdaterId;
    }

    [JsonPropertyName("id")]
    public Guid UserId { get; init; }
    public string UserName { get; init; } = null!;
    public DateTime DateTimeCreated { get; init; }
    public DateTime DateTimeUpdated { get; init; }
    public Guid? CreatorId { get; init; }
    public Guid? LastUpdaterId { get; init; }
    //public UserResponse? Creator { get; init; } = null!;
    //public UserResponse? LastUpdater { get; init; } = null!;
}
