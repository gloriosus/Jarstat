using Jarstat.Domain.Errors;
using Jarstat.Domain.Primitives;
using Jarstat.Domain.Shared;
using System.Text.Json.Serialization;

namespace Jarstat.Domain.Entities;

public class Document : Entity
{
    private Document() { }

    private Document(
        Guid id,
        string displayName,
        string fileName,
        Folder folder,
        string? description,
        DateTime dateTimeCreated,
        DateTime dateTimeUpdated,
        User creator,
        User lastUpdater,
        File? file,
        double sortOrder)
    {
        Id = id;
        DisplayName = displayName;
        FileName = fileName;
        Folder = folder;
        Description = description;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        Creator = creator;
        LastUpdater = lastUpdater;
        File = file;
        SortOrder = sortOrder;
    }

    [JsonConstructor]
    public Document(
        Guid id,
        string displayName,
        string fileName,
        Folder folder,
        string? description,
        DateTime dateTimeCreated,
        DateTime dateTimeUpdated,
        User creator,
        User lastUpdater,
        Guid? fileId,
        double sortOrder)
    {
        Id = id;
        DisplayName = displayName;
        FileName = fileName;
        Folder = folder;
        Description = description;
        DateTimeCreated = dateTimeCreated;
        DateTimeUpdated = dateTimeUpdated;
        Creator = creator;
        LastUpdater = lastUpdater;
        FileId = fileId;
        SortOrder = sortOrder;
    }

    public string DisplayName { get; private set; } = null!; 
    public string FileName { get; private set; } = null!;
    public Guid FolderId { get; private set; }
    public Folder Folder { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? FileId { get; private set; }
    public File? File { get; private set; }
    public double SortOrder { get; private set; }

    //public static Result<Document> Create(
    //    string displayName, 
    //    string fileName,
    //    Folder folder, 
    //    string? description, 
    //    User creator,
    //    File file)
    //{
    //    if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(fileName))
    //        return Result<Document>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue);

    //    if (folder is null || creator is null || file is null)
    //        return Result<Document>.Failure(DomainErrors.ArgumentNullValue);

    //    var lastUpdater = creator;
    //    var document = new Document(
    //        Guid.NewGuid(), 
    //        displayName, 
    //        fileName, 
    //        folder, 
    //        description, 
    //        DateTime.UtcNow, 
    //        DateTime.UtcNow, 
    //        creator, 
    //        lastUpdater, 
    //        file);

    //    return document;
    //}

    public static Result<Document?> Create(
        string displayName,
        string fileName,
        Folder folder,
        string? description,
        User creator,
        Guid? fileId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result<Document?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(displayName), typeof(string).ToString()));

        if (string.IsNullOrWhiteSpace(fileName))
            return Result<Document?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(fileName), typeof(string).ToString()));

        if (folder is null)
            return Result<Document?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(folder), typeof(Folder).ToString()));

        if (creator is null)
            return Result<Document?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(creator), typeof(User).ToString()));

        var lastUpdater = creator;
        var document = new Document(
            Guid.NewGuid(),
            displayName,
            fileName,
            folder,
            description,
            DateTime.UtcNow,
            DateTime.UtcNow,
            creator,
            lastUpdater,
            fileId,
            long.MaxValue);

        return document;
    }

    public Result<Document?> Update(
        string displayName,
        string fileName,
        Folder folder,
        string? description,
        User lastUpdater,
        Guid? fileId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result<Document?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(displayName), typeof(string).ToString()));

        if (string.IsNullOrWhiteSpace(fileName))
            return Result<Document?>.Failure(DomainErrors.ArgumentNullOrWhiteSpaceValue
                .WithParameters(nameof(fileName), typeof(string).ToString()));

        if (folder is null)
            return Result<Document?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(folder), typeof(Folder).ToString()));

        if (lastUpdater is null)
            return Result<Document?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(lastUpdater), typeof(User).ToString()));

        DisplayName = displayName;
        FileName = fileName;
        Folder = folder;
        Description = description;
        LastUpdater = lastUpdater;
        FileId = fileId;

        return this;
    }

    public Result<Document?> WithFile(File file)
    {
        if (file is null)
            return Result<Document?>.Failure(DomainErrors.ArgumentNullValue
                .WithParameters(nameof(file), typeof(File).ToString()));

        var document = new Document(
            this.Id,
            this.DisplayName,
            this.FileName,
            this.Folder,
            this.Description,
            this.DateTimeCreated,
            this.DateTimeUpdated,
            this.Creator,
            this.LastUpdater,
            file,
            this.SortOrder);

        return document;
    }

    public Result<Document?> ChangeSortOrder(double sortOrder)
    {
        if (sortOrder < 0)
            return Result<Document?>.Failure(new Error("Error.ArgumentLessThanZeroValue", "Значение параметра не может быть меньше нуля")
                .WithParameters(nameof(sortOrder), typeof(double).ToString()));

        SortOrder = sortOrder;

        return this;
    }
}
