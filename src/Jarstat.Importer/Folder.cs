namespace Jarstat.Importer;

public class Folder
{
    public string DisplayName { get; set; } = string.Empty;
    public List<Folder> Children { get; set; } = new();
}
