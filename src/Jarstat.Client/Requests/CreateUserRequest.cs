namespace Jarstat.Client.Requests;

public class CreateUserRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Guid? CreatorId { get; set; }
}
