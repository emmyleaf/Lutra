using System.Collections;
using System.Collections.Generic;

namespace Lutra.Utility.Collections;

public static class CollectionExtensions
{
    /// <summary>
    /// Clone this HashSet.
    /// </summary>
    public static HashSet<T> Clone<T>(this HashSet<T> hashSet)
    {
        return new(hashSet, hashSet.Comparer);
    }

    /// <summary>
    /// If this Dictionary already contains the given key, update its value.
    /// Otherwise, add the key and value to this Dictionary.
    /// </summary>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }
    }

    /// <summary>
    /// Returns true if this collection has a Count of 0.
    /// </summary>
    public static bool IsEmpty(this ICollection collection)
    {
        return collection.Count == 0;
    }

    /// <summary>
    /// Returns true if this collection has a Count larger than 0.
    /// </summary>
    public static bool IsNotEmpty(this ICollection collection)
    {
        return collection.Count > 0;
    }

    /// <summary>
    /// Returns a randomly selected item from this IReadOnlyList<T> or default.
    /// </summary>
    public static T RandomElement<T>(this IReadOnlyList<T> list)
    {
        if (list.Count == 0) return default(T);

        return list[Rand.Int(list.Count)];
    }
}
