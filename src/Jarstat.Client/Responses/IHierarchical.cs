namespace Jarstat.Client.Responses;

public interface IHierarchical<T>
{
    Guid Id { get; }
    IEnumerable<T> Children { get; set; }
}
