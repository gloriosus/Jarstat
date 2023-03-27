using Jarstat.Domain.Errors;
using Jarstat.Domain.Primitives;
using Jarstat.Domain.Records;
using Jarstat.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Jarstat.Domain.Entities;

public sealed class Folder : Entity
{
    private Folder() { }

    [JsonConstructor]
    public Folder(
        Guid id,
        string displayName,
        string virtualPath,
        Folder? parent,
        DateTime dateTimeCreated,
        DateTime dateTimeUpdated,
        User creator,
        User lastUpdater,
        double sortOrder)
    {
        Id = id;
        DisplayName = displayName;
        VirtualPath = virtualPath;
        Parent = parent;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        Creator = creator;
        LastUpdater = lastUpdater;
        SortOrder = sortOrder;
    }

    public string DisplayName { get; private set; } = null!;
    public string VirtualPath { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public Folder? Parent { get; private set; }
    public double SortOrder { get; private set; }

    [NotMapped]
    public List<Folder> Children { get; set; } = new();

    public static explicit operator Item?(Folder? folder) =>
        folder is null ? null : new Item(folder.Id,
                                           folder.DisplayName,
                                           folder.ParentId,
                                           "Folder",
                                           folder.DateTimeCreated,
                                           folder.DateTimeUpdated,
                                           folder.SortOrder);

    public static Result<Folder?> Create(string displayName, string virtualPath, Folder? parent, User creator)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result<Folder?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(displayName), typeof(string).ToString()));

        if (string.IsNullOrWhiteSpace(virtualPath))
            return Result<Folder?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(virtualPath), typeof(string).ToString()));

        if (creator is null)
            return Result<Folder?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(creator), typeof(User).ToString()));

        var folder = new Folder(
            Guid.NewGuid(), 
            displayName, 
            virtualPath, 
            parent, 
            DateTime.UtcNow, 
            DateTime.UtcNow, 
            creator, 
            creator,
            long.MaxValue);

        return folder;
    }

    public Result<Folder?> Update(string displayName, string virtualPath, Folder? parent, User lastUpdater)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result<Folder?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(displayName), typeof(string).ToString()));

        if (string.IsNullOrWhiteSpace(virtualPath))
            return Result<Folder?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(virtualPath), typeof(string).ToString()));

        if (lastUpdater is null)
            return Result<Folder?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(lastUpdater), typeof(User).ToString()));

        DisplayName = displayName;
        VirtualPath = virtualPath;
        Parent = parent;
        DateTimeUpdated = DateTime.UtcNow;
        LastUpdater = lastUpdater;

        return this;
    }

    public Result<Folder?> ChangeSortOrder(double sortOrder)
    {
        if (sortOrder < 0)
            return Result<Folder?>.Failure(new Error("Error.ArgumentLessThanZeroValue", "Значение параметра не может быть меньше нуля")
                .WithParameters(nameof(sortOrder), typeof(double).ToString()));

        SortOrder = sortOrder;

        return this;
    }
}
