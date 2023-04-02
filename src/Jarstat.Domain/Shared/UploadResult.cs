namespace Jarstat.Domain.Shared;

public sealed record UploadResult(string? FileName, Guid? FileId)
{
    public static UploadResult Empty = new UploadResult(null, null);
}
