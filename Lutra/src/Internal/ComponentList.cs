using System.Collections;
using System.Collections.Generic;
using Lutra.Utility.Collections;

namespace Lutra;

internal class ComponentList : IManagedList<Component>
{
    private const int INITIAL_SIZE = 16;

    private static readonly Comparison<Component> CompareOrder = (a, b) => a.Order - b.Order;

    private readonly Entity Entity;
    private readonly LutraList<Component> components = new(INITIAL_SIZE);
    private readonly LutraList<Component> toAdd = new(INITIAL_SIZE);
    private readonly LutraList<Component> toRemove = new(INITIAL_SIZE);
    private readonly HashSet<Component> componentSet = new(INITIAL_SIZE);
    private bool unsorted;

    #region Internal

    internal ComponentList(Entity entity)
    {
        Entity = entity;
    }

    internal void UpdateLists()
    {
        if (toAdd.Count > 0)
        {
            for (int i = 0; i < toAdd.Count; i++)
            {
                var component = toAdd[i];
                if (componentSet.Add(component))
                {
                    components.Add(component);
                    component.InternalAdded(Entity);
                }
            }

            unsorted = true;

            toAdd.Clear();
        }

        if (toRemove.Count > 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                var component = toRemove[i];
                if (componentSet.Remove(component))
                {
                    components.Remove(component);
                    component.InternalRemoved();
                }
            }

            toRemove.Clear();
        }

        if (unsorted)
        {
            components.StableSort(CompareOrder);
            unsorted = false;
        }
    }

    internal void Add(Component component)
    {
        if (!componentSet.Contains(component))
        {
            toAdd.Add(component);
        }
    }

    internal void Remove(Component component)
    {
        if (componentSet.Contains(component))
        {
            toRemove.Add(component);
        }
    }

    #endregion

    #region Interface Implementations

    public void MarkUnsorted()
    {
        unsorted = true;
    }

    public int Count => components.Count;

    public Component this[int index] => (index < 0 || index >= components.Count) ? null : components[index];

    public IEnumerator<Component> GetEnumerator() => components.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => components.GetEnumerator();

    #endregion
}
