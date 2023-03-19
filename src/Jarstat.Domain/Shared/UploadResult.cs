using System.Text.Json.Serialization;

namespace Jarstat.Domain.Shared;

public class UploadResult
{
    public string? FileName { get; set; }
    public string? StoredFileName { get; set; }
}
