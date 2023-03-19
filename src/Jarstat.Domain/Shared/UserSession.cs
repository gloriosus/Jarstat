namespace Jarstat.Domain.Shared;

public class UserSession
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public int ExpiresIn { get; set; }
    public DateTime ExpiryTimeStamp { get; set; }
}
