namespace Jarstat.Client.Abstractions;

public interface IHierarchical<T>
{
    Guid Id { get; }
    IEnumerable<T> Children { get; set; }
}
