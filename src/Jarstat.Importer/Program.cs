using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Text.Json;
using System.Text;

namespace Jarstat.Importer;

public class Program
{
    private static IDbConnection db;

    private static async Task Main()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();

        var connectionString = config.GetSection("ConnectionString").Value;
        var sourceFolder = config.GetSection("SourceFolder").Value;
        var creatorUserName = config.GetSection("CreatorUserName").Value;
        var subjectDefinitionsFolder = config.GetSection("SubjectDefinitionsFolder").Value;

        if (connectionString is null)
        {
            Console.WriteLine("Не найдены настройки подключения к базе данных, проверьте наличие параметра 'ConnectionString' в файле appsettings.json");
            Console.ReadKey();
            return;
        }

        if (sourceFolder is null)
        {
            Console.WriteLine("Не найден путь к папке с документами, проверьте наличие параметра 'SourceFolder' в файле appsettings.json");
            Console.ReadKey();
            return;
        }

        if (creatorUserName is null)
        {
            Console.WriteLine("Не найдено имя пользователя веб-приложения, проверьте наличие параметра 'CreatorUserName' в файле appsettings.json");
            Console.ReadKey();
            return;
        }

        if (subjectDefinitionsFolder is null)
        {
            Console.WriteLine("Не найден путь к определениям тематик в формате JSON, проверьте наличие параметра 'SubjectDefinitionsFolder' в файле appsettings.json");
            Console.ReadKey();
            return;
        }

        db = new NpgsqlConnection(connectionString);

        Console.WriteLine("Введите команду: 1) импортировать документы, 2) переименовать папки");
        string? command = Console.ReadLine();

        switch (command)
        {
            case "1":
                var creatorId = await db.QueryFirstAsync<Guid>("SELECT \"Id\" FROM \"AspNetUsers\" WHERE \"UserName\" = @CreatorUserName", new { CreatorUserName = creatorUserName });
                var rootId = await CreateFolder(sourceFolder, null, creatorId, long.MaxValue);
                await ImportAsync(sourceFolder, rootId, creatorId);
                Console.WriteLine("Операция импорта документов завершена");
                break;
            case "2":
                await RenameFolders(subjectDefinitionsFolder);
                Console.WriteLine("Операция переименования папок завершена");
                break;
            case null:
                Console.WriteLine("Отмена операции");
                break;
        }

        Console.ReadKey();
    }

    #region Rename folders
    private static async Task RenameFolders(string jsonPath)
    {
        foreach (var file in Directory.GetFiles(jsonPath, "*.json"))
        {
            string json = await File.ReadAllTextAsync(file, Encoding.UTF8);
            var rootFolder = JsonSerializer.Deserialize<Folder>(json);

            if (rootFolder is null) return;

            await RenameRecursively(rootFolder, null);
        }
    }

    private static async Task RenameRecursively(Folder folder, Guid? parentId)
    {
        var searchKey = folder.DisplayName.Split(' ')[0];

        Guid folderId = default;

        if (parentId is null)
        {
            folderId = await db.QueryFirstOrDefaultAsync<Guid>("SELECT \"Id\" FROM \"Folders\" WHERE \"DisplayName\" = @SearchKey AND \"ParentId\" IS NULL", new { SearchKey = searchKey });
        }
        else
        {
            folderId = await db.QueryFirstOrDefaultAsync<Guid>("SELECT \"Id\" FROM \"Folders\" WHERE \"DisplayName\" = @SearchKey AND \"ParentId\" = @ParentId", new { SearchKey = searchKey, ParentId = parentId });
        }

        await db.ExecuteAsync("UPDATE \"Folders\" SET \"DisplayName\" = @DisplayName WHERE \"Id\" = @FolderId", new { DisplayName = folder.DisplayName, FolderId = folderId });

        foreach (var subfolder in folder.Children)
            await RenameRecursively(subfolder, folderId);
    }
    #endregion

    #region Import folders and documents
    private static async Task<Guid> CreateFolder(string path, Guid? parentId, Guid creatorId, double sortOrder)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        Guid id = Guid.NewGuid();
        string displayName = directoryInfo.Name;
        string virtualPath = "none";
        DateTime dateTimeCreated = DateTime.UtcNow;
        DateTime dateTimeUpdated = DateTime.UtcNow;
        Guid lastUpdaterId = creatorId;

        int rowsAffected = await db.ExecuteAsync("INSERT INTO \"Folders\" (\"Id\", \"DisplayName\", \"VirtualPath\", \"ParentId\", \"DateTimeCreated\", \"DateTimeUpdated\", \"CreatorId\", \"LastUpdaterId\", \"SortOrder\") VALUES (@Id, @DisplayName, @VirtualPath, @ParentId, @DateTimeCreated, @DateTimeUpdated, @CreatorId, @LastUpdaterId, @SortOrder)", new { Id = id, DisplayName = displayName, VirtualPath = virtualPath, ParentId = parentId, DateTimeCreated = dateTimeCreated, DateTimeUpdated = dateTimeUpdated, CreatorId = creatorId, LastUpdaterId = lastUpdaterId, SortOrder = sortOrder });

        return id;
    }

    private static async Task CreateDocument(string path, Guid folderId, Guid creatorId, double sortOrder)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        Guid id = Guid.NewGuid();
        string displayName = Path.GetFileNameWithoutExtension(directoryInfo.Name);
        string fileName = directoryInfo.Name;
        string? description = null;
        Guid? fileId = await CreateFile(path, creatorId, directoryInfo.LastWriteTimeUtc);
        DateTime dateTimeCreated = directoryInfo.LastWriteTimeUtc;
        DateTime dateTimeUpdated = directoryInfo.LastWriteTimeUtc;
        Guid lastUpdaterId = creatorId;

        int rowsAffected = await db.ExecuteAsync("INSERT INTO \"Documents\" (\"Id\", \"DisplayName\", \"FileName\", \"FolderId\", \"Description\", \"FileId\", \"DateTimeCreated\", \"DateTimeUpdated\", \"CreatorId\", \"LastUpdaterId\", \"SortOrder\") VALUES (@Id, @DisplayName, @FileName, @FolderId, @Description, @FileId, @DateTimeCreated, @DateTimeUpdated, @CreatorId, @LastUpdaterId, @SortOrder)", new { Id = id, DisplayName = displayName, FileName = fileName, FolderId = folderId, Description = description, FileId = fileId, DateTimeCreated = dateTimeCreated, DateTimeUpdated = dateTimeUpdated, CreatorId = creatorId, LastUpdaterId = lastUpdaterId, SortOrder = sortOrder });
    }

    private static async Task<Guid> CreateFile(string path, Guid creatorId, DateTime lastWriteTimeUtc)
    {
        Guid id = Guid.NewGuid();
        byte[] value = File.ReadAllBytes(path);
        DateTime dateTimeCreated = lastWriteTimeUtc;
        DateTime dateTimeUpdated = lastWriteTimeUtc;
        Guid lastUpdaterId = creatorId;

        int rowsAffected = await db.ExecuteAsync("INSERT INTO \"Files\" (\"Id\", \"Value\", \"DateTimeCreated\", \"DateTimeUpdated\", \"CreatorId\", \"LastUpdaterId\") VALUES (@Id, @Value, @DateTimeCreated, @DateTimeUpdated, @CreatorId, @LastUpdaterId)", new { Id = id, Value = value, DateTimeCreated = dateTimeCreated, DateTimeUpdated = dateTimeUpdated, CreatorId = creatorId, LastUpdaterId = lastUpdaterId });

        return id;
    }

    private static async Task ImportAsync(string path, Guid parentId, Guid creatorId)
    {
        if (!IsFolder(path))
            return;

        double sortOrder = long.MaxValue;

        foreach (var item in Directory.GetFileSystemEntries(path))
        {
            Guid id = default;

            switch (IsFolder(item))
            {
                case true:
                    id = await CreateFolder(item, parentId, creatorId, sortOrder);
                    break;
                case false:
                    await CreateDocument(item, parentId, creatorId, sortOrder);
                    break;
            }

            sortOrder = sortOrder / 2;
            await ImportAsync(item, id, creatorId);
        }
    }

    private static bool IsFolder(string path)
    {
        return Directory.Exists(path);
    }
    #endregion
}