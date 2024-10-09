using System.Collections;
using System.Collections.Generic;

namespace Lutra.Utility.Collections;

/// <summary>
/// Implements a variable-size list using an array to store elements.
/// The internal array can be accessed using the Items member. Be careful!
/// Also implements the stable sorting algorithms from Lutra.Utility.Sorting.
/// </summary>
/// <remarks>
/// Construct a new LutraList. The default initial size is 64.
/// </remarks>
public class LutraList<T>(int initialSize = LutraList<T>.INITIAL_SIZE) : IList<T>, IReadOnlyList<T>
{
    private const int INITIAL_SIZE = 64;
    private const int GROW_SIZE = 16;

    public T[] Items = new T[initialSize];
    private T[] workArray;
    private int count;

    /// <summary>
    /// Remove the last item from this list and return it.
    /// </summary>
    public T RemoveLast()
    {
        count -= 1;
        T item = Items[count];
        Items[count] = default;
        return item;
    }

    /// <summary>
    /// Sorts the whole list using an insertion sort.
    /// This sorting algorithm is stable, but is not performant for larger lists.
    /// </summary>
    public void InsertionSort(Comparison<T> comparison)
    {
        if (count > 1)
        {
            Sorting.InsertionSort(Items, 0, count, comparison);
        }
    }

    /// <summary>
    /// Sorts the whole list using a bottom-up merge sort.
    /// This sorting algorithm is stable.
    /// It allocates and reuses a working array of the same size as Items, stored within this class.
    /// </summary>
    public void MergeSort(Comparison<T> comparison)
    {
        if (count > 1)
        {
            workArray ??= new T[Items.Length];
            Sorting.MergeSort(Items, workArray, count, comparison);
        }
    }

    /// <summary>
    /// Sorts the whole list using a hybrid insertion and bottom-up merge sort.
    /// This sorting algorithm is stable.
    /// It allocates and reuses a working array of the same size as Items, stored within this class.
    /// </summary>
    public void StableSort(Comparison<T> comparison)
    {
        if (count > 1)
        {
            workArray ??= new T[Items.Length];
            Sorting.StableSort(Items, workArray, count, comparison);
        }
    }

    /// <summary>
    /// Sorts the whole list using Array.Sort.
    /// This sorting algorithm is not stable, but can be quicker than the alternatives.
    /// </summary>
    public void Sort(IComparer<T> comparer)
    {
        if (count > 1)
        {
            Array.Sort(Items, 0, count, comparer);
        }
    }

    /// <summary>
    /// Get Items as a ReadOnlySpan<T>.
    /// </summary>
    public ReadOnlySpan<T> GetReadOnlySpan()
    {
        return new ReadOnlySpan<T>(Items, 0, Count);
    }

    #region Interface Implementations

    public int Count => count;

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }

    public void Add(T item)
    {
        int length = Items.Length;
        if (count >= length)
        {
            Array.Resize(ref Items, length + GROW_SIZE);
            if (workArray != null)
            {
                Array.Resize(ref workArray, length + GROW_SIZE);
            }
        }

        Items[count] = item;
        count += 1;
    }

    public void Clear()
    {
        Array.Clear(Items, 0, count);
        count = 0;
    }

    public bool Contains(T item)
    {
        return count != 0 && Array.IndexOf(Items, item, 0, count) != -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(Items, 0, array, arrayIndex, count);
    }

    public int IndexOf(T item)
    {
        return Array.IndexOf(Items, item, 0, count);
    }

    public void Insert(int index, T item)
    {
        ((IList<T>)Items).Insert(index, item);
        count += 1;
    }

    public bool Remove(T item)
    {
        int index = Array.IndexOf(Items, item, 0, count);

        if (index >= 0)
        {
            int nextIndex = index + 1;
            if (nextIndex < count)
            {
                Array.Copy(Items, nextIndex, Items, index, count - nextIndex);
            }

            count -= 1;
            Items[count] = default;
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }

        int nextIndex = index + 1;
        if (nextIndex < count)
        {
            Array.Copy(Items, nextIndex, Items, index, count - nextIndex);
        }

        count -= 1;
        Items[count] = default;
    }

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    #endregion

    #region Enumerator

    public struct Enumerator : IEnumerator<T>
    {
        private readonly LutraList<T> list;
        private int index;
        private T current;

        internal Enumerator(LutraList<T> list)
        {
            this.list = list;
            index = 0;
            current = default;
        }

        public readonly T Current => current;

        readonly object IEnumerator.Current => current;

        public readonly void Dispose() { }

        public bool MoveNext()
        {
            if (index < list.count)
            {
                current = list.Items[index];
                index += 1;
                return true;
            }

            index = list.count + 1;
            current = default;
            return false;
        }

        void IEnumerator.Reset()
        {
            index = 0;
            current = default;
        }
    }

    #endregion
}
