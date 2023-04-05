using Jarstat.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace Jarstat.Client.Responses;

public record UserResponse : IDefault<UserResponse>
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

    public static UserResponse? Default => null;

    [JsonPropertyName("id")]
    public Guid UserId { get; init; }
    public string UserName { get; init; } = null!;
    public DateTime DateTimeCreated { get; init; }
    public DateTime DateTimeUpdated { get; init; }
    public Guid? CreatorId { get; init; }
    public Guid? LastUpdaterId { get; init; }
}
