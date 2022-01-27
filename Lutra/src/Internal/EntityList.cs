using System.Collections;
using System.Collections.Generic;
using Lutra.Utility.Collections;

namespace Lutra;

internal class EntityList : IManagedList<Entity>
{
    private const int INITIAL_SIZE = 64;

    private static readonly Comparison<Entity> CompareOrder = (a, b) => a.Order - b.Order;

    private Scene Scene;
    private readonly LutraList<Entity> entities = new(INITIAL_SIZE);
    private readonly LutraList<Entity> toAdd = new(INITIAL_SIZE);
    private readonly LutraList<Entity> toRemove = new(INITIAL_SIZE);
    private readonly HashSet<Entity> entitySet = new(INITIAL_SIZE);
    private bool unsorted;

    #region Internal

    internal EntityList(Scene scene)
    {
        Scene = scene;
    }

    internal void UpdateLists()
    {
        if (toAdd.Count > 0)
        {
            for (int i = 0; i < toAdd.Count; i++)
            {
                var entity = toAdd[i];
                if (entitySet.Add(entity))
                {
                    entities.Add(entity);
                    entity.InternalAdded(Scene);
                }
            }

            unsorted = true;

            toAdd.Clear();
        }

        if (toRemove.Count > 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                var entity = toRemove[i];
                if (entitySet.Remove(entity))
                {
                    entities.Remove(entity);
                    entity.InternalRemoved();
                }
            }

            toRemove.Clear();
        }

        if (unsorted)
        {
            entities.StableSort(CompareOrder);
            unsorted = false;
        }
    }

    internal void Add(Entity entity)
    {
        if (!entitySet.Contains(entity))
        {
            toAdd.Add(entity);
        }
    }

    internal void Remove(Entity entity)
    {
        if (entitySet.Contains(entity))
        {
            toRemove.Add(entity);
        }
    }

    internal void End()
    {
        foreach (var entity in entities) entity.InternalRemoved();

        entities.Clear();
        entitySet.Clear();
        toAdd.Clear();
        toRemove.Clear();
        Scene = null;
    }

    #endregion

    #region Interface Implementations

    public void MarkUnsorted()
    {
        unsorted = true;
    }

    public int Count => entities.Count;

    public Entity this[int index] => (index < 0 || index >= entities.Count) ? null : entities[index];

    public IEnumerator<Entity> GetEnumerator() => entities.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => entities.GetEnumerator();

    #endregion
}
