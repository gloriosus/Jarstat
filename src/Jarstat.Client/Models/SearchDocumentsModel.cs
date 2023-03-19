using Jarstat.Domain.Entities;

namespace Jarstat.Client.Models;

public class SearchDocumentsModel
{
    public string? DocumentName { get; set; } = null;
    public string? FolderValue { get; set; } = null;
    public string? SubjectName { get; set; } = null;
    public string? RegionName { get; set; } = null;
    public string? DateTimeCreatedValue { get; set; } = null;
    public string? DateTimeUpdatedValue { get; set; } = null;
}
