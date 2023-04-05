using Jarstat.Domain.Abstractions;

namespace Jarstat.Domain.Shared;

public class UserSession : IDefault<UserSession>
{
    public static UserSession? Default => null;

    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Token { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public DateTime ExpiryTimeStamp { get; set; }
}
