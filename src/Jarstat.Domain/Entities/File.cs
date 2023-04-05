using Jarstat.Domain.Abstractions;
using Jarstat.Domain.Errors;
using Jarstat.Domain.Primitives;
using Jarstat.Domain.Shared;
using System.Text.Json.Serialization;

namespace Jarstat.Domain.Entities;

public sealed class File : Entity, IDefault<File>
{
    private File() { }

    [JsonConstructor]
    public File(
        Guid id, 
        byte[] value, 
        DateTime dateTimeCreated, 
        DateTime dateTimeUpdated, 
        User creator, 
        User lastUpdater)
    {
        Id = id;
        Value = value;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        Creator = creator;
        LastUpdater = lastUpdater;
    }

    public byte[] Value { get; private set; } = null!;

    public static File? Default => null;

    public static Result<File> Create(byte[] value, User creator)
    {
        if (value is null)
            return Result<File>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(value), typeof(byte[]).ToString()));

        if (value.Length == 0)
            return Result<File>.Failure(DomainErrors.ArgumentArrayLengthIsZeroValue
                .WithParameters(nameof(value), typeof(byte[]).ToString()));

        if (creator is null)
            return Result<File>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(creator), typeof(User).ToString()));

        var content = new File(Guid.NewGuid(), value, DateTime.UtcNow, DateTime.UtcNow, creator, creator);

        return content;
    }
}
