using System.Collections.Generic;

namespace Lutra.Utility.Collections;

/// <summary>
/// Represents an interface for internal lists managed within Lutra.
/// </summary>
public interface IManagedList<T> : IReadOnlyList<T>
{
    /// <summary>
    /// Mark this list as unsorted. The next time it is updated, the list will be sorted.
    /// </summary>
    void MarkUnsorted();
}
