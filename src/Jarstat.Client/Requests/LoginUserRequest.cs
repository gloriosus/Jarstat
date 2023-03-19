namespace Jarstat.Client.Requests;

public class LoginUserRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
