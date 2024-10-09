using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lutra.Utility.Collections;

/// <summary>
/// A simple LinkedHashSet. A collection with uniqueness and order.
/// Internally uses a Dictionary and a LinkedList.
/// </summary>
public class LinkedHashSet<T>(IEqualityComparer<T> comparer) : ICollection<T>, IReadOnlyList<T>
{
    private readonly Dictionary<T, LinkedListNode<T>> dictionary = new(comparer);
    private readonly LinkedList<T> linkedList = new();

    public LinkedHashSet() : this(EqualityComparer<T>.Default) { }

    public bool Add(T item)
    {
        if (dictionary.ContainsKey(item)) return false;
        LinkedListNode<T> node = linkedList.AddLast(item);
        dictionary.Add(item, node);
        return true;
    }

    public T First()
    {
        var node = linkedList.First;
        return (node != null) ? node.Value : default;
    }

    public T Last()
    {
        var node = linkedList.Last;
        return (node != null) ? node.Value : default;
    }

    #region Interface Implementations

    public T this[int index] => (index >= 0 && index < dictionary.Count) ? linkedList.ElementAt(index) : default;

    public int Count => dictionary.Count;

    public bool IsReadOnly => false;

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public void Clear()
    {
        linkedList.Clear();
        dictionary.Clear();
    }

    public bool Contains(T item)
    {
        return dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        linkedList.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        if (dictionary.Remove(item, out var node))
        {
            linkedList.Remove(node);
            return true;
        }
        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return linkedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
