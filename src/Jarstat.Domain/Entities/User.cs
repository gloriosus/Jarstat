using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Jarstat.Domain.Entities;

public class User : IdentityUser<Guid>, IDefault<User>
{
    public static User? Default => null;

    public DateTime DateTimeCreated { get; protected init; }
    public DateTime DateTimeUpdated { get; protected set; }
    public Guid? CreatorId { get; protected init; }
    public Guid? LastUpdaterId { get; protected set; }

    [JsonIgnore]
    public User? Creator { get; protected init; }

    [JsonIgnore]
    public User? LastUpdater { get; protected set; }

    public static Result<User> Create(string username, User? creator)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result<User>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(username), typeof(string).ToString()));

        return new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            DateTimeCreated = DateTime.UtcNow,
            DateTimeUpdated = DateTime.UtcNow,
            Creator = creator,
            LastUpdater = creator
        };
    }

    public User Update(User lastUpdater)
    {
        DateTimeUpdated = DateTime.UtcNow;
        LastUpdater = lastUpdater;
        return this;
    }
}
