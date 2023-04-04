using Jarstat.Domain.Abstractions;
using System.Collections;

namespace Jarstat.Client.Responses;

public class Collection<T> : IList<T>, IDefault<Collection<T>>
{
    private static readonly Collection<T> Empty = new Collection<T>() { _items = new List<T>().AsReadOnly() };

    private IList<T> _items = new List<T>();

    public T this[int index] 
    { 
        get => _items[index]; 
        set => _items[index] = value; 
    }

    public static Collection<T>? Default => Empty;

    public int Count => _items.Count;

    public bool IsReadOnly => _items.IsReadOnly;

    public void Add(T item)
    {
        _items.Add(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _items.Insert(index, item);
    }

    public bool Remove(T item)
    {
        return _items.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.AsEnumerable().GetEnumerator();
    }
}
