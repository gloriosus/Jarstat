using Jarstat.Domain.Abstractions;

namespace Jarstat.Domain.Shared;

public sealed record UploadValue(string? FileName, Guid? FileId) : IDefault<UploadValue>
{
    public static UploadValue? Empty = new UploadValue(null, null);
    public static UploadValue? Default => Empty;
}
