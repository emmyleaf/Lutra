using System.Collections;
using System.Collections.Generic;
using Lutra.Utility.Collections;

namespace Lutra;

internal class GraphicList : IManagedList<Graphic>
{
    private const int INITIAL_SIZE = 128;

    // TODO: Fix all da examples layers...
    private static readonly Comparison<Graphic> CompareLayer = (a, b) => b.Layer - a.Layer;

    private readonly LutraList<Graphic> graphics = new(INITIAL_SIZE);
    private readonly HashSet<Graphic> graphicSet = new(INITIAL_SIZE);
    private bool unsorted;

    #region Internal

    internal void UpdateLists()
    {
        if (unsorted)
        {
            graphics.StableSort(CompareLayer);
            unsorted = false;
        }
    }

    internal void Add(Graphic graphic)
    {
        if (graphicSet.Add(graphic))
        {
            graphics.Add(graphic);
            unsorted = true;
        }
    }

    internal void Remove(Graphic graphic)
    {
        if (graphicSet.Remove(graphic))
        {
            graphics.Remove(graphic);
        }
    }

    internal void RenderAll()
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            graphics[i].InternalRender();
        }
    }

    #endregion

    #region Interface Implementations

    public void MarkUnsorted()
    {
        unsorted = true;
    }

    public int Count => graphics.Count;

    public Graphic this[int index] => (index < 0 || index >= graphics.Count) ? null : graphics[index];

    public IEnumerator<Graphic> GetEnumerator() => graphics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => graphics.GetEnumerator();

    #endregion
}
