namespace Jarstat.Client.Requests;

public class SearchUsersRequest
{
    public string? UserName { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
