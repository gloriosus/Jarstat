using Jarstat.Domain.Abstractions;

namespace Jarstat.Domain.Shared;

public sealed record UploadValue(string? FileName, Guid? FileId) : IDefault<UploadValue>
{
    public static UploadValue? Default => new UploadValue(null, null);
}
