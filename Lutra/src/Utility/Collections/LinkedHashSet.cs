using System.Collections;
using System.Collections.Generic;

namespace Lutra.Utility.Collections;

/// <summary>
/// A simple LinkedHashSet. A collection with uniqueness and order.
/// Internally uses a Dictionary and a LinkedList.
/// </summary>
public class LinkedHashSet<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private readonly Dictionary<T, LinkedListNode<T>> dictionary;
    private readonly LinkedList<T> linkedList;

    public LinkedHashSet() : this(EqualityComparer<T>.Default) { }

    public LinkedHashSet(IEqualityComparer<T> comparer)
    {
        dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
        linkedList = new LinkedList<T>();
    }

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
        return (node != null) ? node.Value : default(T);
    }

    public T Last()
    {
        var node = linkedList.Last;
        return (node != null) ? node.Value : default(T);
    }

    #region Interface Implementations

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
