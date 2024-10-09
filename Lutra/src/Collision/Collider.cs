using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Lutra.Utility;

namespace Lutra.Collision
{
    public abstract class Collider
    {
        #region Public Fields

        /// <summary>
        /// The X position of the Collider relative to the Entity.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y position of the Collider relative to the Entity.
        /// </summary>
        public float Y;

        /// <summary>
        /// The OriginX of the Collider.
        /// </summary>
        public float OriginX;

        /// <summary>
        /// The OriginY of the Collider.
        /// </summary>
        public float OriginY;

        /// <summary>
        /// The entity that this collider is parented to.
        /// </summary>
        public Entity Entity;

        /// <summary>
        /// Determines if this collider is collidable.  If false, it will not register collisions with
        /// other colliders, but can still check for collisions with other colliders.
        /// </summary>
        public bool Collidable = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// The tags that this Collider is registered with.
        /// </summary>
        public List<int> Tags { get; protected set; }

        /// <summary>
        /// The width of the Collider.
        /// </summary>
        public virtual float Width { get; set; }

        /// <summary>
        /// The height of the Collider.
        /// </summary>
        public virtual float Height { get; set; }

        /// <summary>
        /// The X position of the center of the Collider.
        /// </summary>
        public virtual float CenterX => X + Entity.X - OriginX + HalfWidth;

        /// <summary>
        /// The Y position of the center of the Collider.
        /// </summary>
        public virtual float CenterY => Y + Entity.Y - OriginY + HalfHeight;

        /// <summary>
        /// The X position of the left side of the Collider.
        /// </summary>
        public virtual float Left => X + Entity.X - OriginX;

        /// <summary>
        /// The X position of the right side of the Collider.
        /// </summary>
        public virtual float Right => X + Entity.X + Width - OriginX;

        /// <summary>
        /// The Y position of the top of the Collider.
        /// </summary>
        public virtual float Top => Y + Entity.Y - OriginY;

        /// <summary>
        /// The Y position of the bottom of the Collider.
        /// </summary>
        public virtual float Bottom => Y + Entity.Y + Height - OriginY;

        /// <summary>
        /// Half of the Collider's height.
        /// </summary>
        public float HalfHeight => Height / 2f;

        /// <summary>
        /// Half of the Collider's width.
        /// </summary>
        public float HalfWidth => Width / 2f;

        #endregion

        private static CollisionSystem CollisionSystem => CollisionSystem.Instance;

        #region Constructors

        internal Collider()
        {
            Tags = [];
            Width = 0;
            Height = 0;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks for a collision against the specified tags and returns true or false.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap(float x, float y, params int[] tags)
        {
            return (Collide(x, y, tags) != null);
        }

        /// <summary>
        /// Checks for a collision against a specific Collider and returns true or false.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="c">The Collider to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap(float x, float y, Collider c)
        {
            return (Collide(x, y, c) != null);
        }

        /// <summary>
        /// Checks for a collision against a specific Entity and returns true or false.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="e">The Collider to check.</param>
        /// <returns>True of there was a collision.</returns>
        public bool Overlap(float x, float y, Entity e)
        {
            return (Collide(x, y, e) != null);
        }

        /// <summary>
        /// Checks for a collision against a list of Collliders and returns true or false.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The list of colliders to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap(float x, float y, List<int> tags)
        {
            return (Collide(x, y, tags) != null);
        }

        /// <summary>
        /// Checks for a collision against the specified tags and returns true or false.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap(float x, float y, params Enum[] tags)
        {
            return (Collide(x, y, tags) != null);
        }

        /// <summary>
        /// Checks for a collision against the specified tags and returns true or false.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap(float x, float y, List<Enum> tags)
        {
            return (Collide(x, y, tags) != null);
        }

        /// <summary>
        /// Checks for a collision against a list of Entities and all of their colliders.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="entities">The Entities to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap(float x, float y, List<Entity> entities)
        {
            return (Collide(x, y, entities) != null);
        }

        /// <summary>
        /// Checks for a collision against a list of Entities and all of their colliders.
        /// </summary>
        /// <typeparam name="T">The type of entity to check.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="entities">The Entities to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool Overlap<T>(float x, float y, List<T> entities) where T : Entity
        {
            return (Collide(x, y, entities.ToList<Entity>()) != null);
        }

        /// <summary>
        /// Checks for a collision against a list of Entities and all of their colliders.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="entities">The Entities to check.</param>
        /// <returns>True if there was a collision.</returns>
        public Collider Collide(float x, float y, List<Entity> entities)
        {
            foreach (var e in entities)
            {
                var c = Collide(x, y, e);
                if (c != null) return c;
            }
            return null;
        }

        public Collider Collide<T>(float x, float y, List<T> entities) where T : Entity
        {
            return Collide(x, y, entities.ToList<Entity>());
        }

        /// <summary>
        /// Checks for a collision against specified tags.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The collider that was hit first.</returns>
        public Collider Collide(float x, float y, params int[] tags)
        {
            if (Entity == null) return null;
            if (Entity.Scene == null) return null;

            float tempX = Entity.X, tempY = Entity.Y;
            Entity.X = x;
            Entity.Y = y;

            if (tags.Length == 0)
            {
                tags = [.. CollisionSystem.KnownColliderTags];
            }

            var values = new List<Collider>();
            CollisionSystem.ColliderQuadTree.FindCollisions(this, ref values);

            foreach (var c in values)
            {
                if (c.Entity != null)
                {
                    if (!c.Entity.Collidable) continue;
                    if (!c.Collidable) continue;
                    if (c.Entity == Entity) continue;
                }
                else
                {
                    continue;
                }
                foreach (int t in tags)
                {
                    if (c.HasTag(t))
                    {
                        if (CollisionSystem.OverlapTest(this, c))
                        {
                            Entity.X = tempX;
                            Entity.Y = tempY;
                            return c;
                        }
                    }
                }
            }

            Entity.X = tempX;
            Entity.Y = tempY;
            return null;
        }

        /// <summary>
        /// Checks for a collision against specified tags.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The collider that was hit first.</returns>
        public Collider Collide(float x, float y, params Enum[] tags)
        {
            return Collide(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Checks for a collision with a specific collider.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="c">The collider to check for.</param>
        /// <returns>The collider that was hit first.</returns>
        public Collider Collide(float x, float y, Collider c)
        {
            if (Entity == null) return null;
            if (Entity.Scene == null) return null;
            if (CollisionSystem == null) return null;

            if (!c.Entity.Collidable) return null;
            if (!c.Collidable) return null;

            float tempX = Entity.X, tempY = Entity.Y;
            Entity.X = x;
            Entity.Y = y;

            if (CollisionSystem.OverlapTest(this, c))
            {
                Entity.X = tempX;
                Entity.Y = tempY;
                return c;
            }

            Entity.X = tempX;
            Entity.Y = tempY;
            return null;
        }

        /// <summary>
        /// Checks for a collision with a specific entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="e">The entity to check for.</param>
        /// <returns>The collider that was hit.</returns>
        public Collider Collide(float x, float y, Entity e)
        {
            if (Entity == null) return null;
            if (Entity.Scene == null) return null;
            if (CollisionSystem == null) return null;
            if (Entity == e) return null; // Can't collide with self
            if (!Entity.Collidable) return null;

            float tempX = Entity.X, tempY = Entity.Y;
            Entity.X = x;
            Entity.Y = y;

            foreach (Collider c in e.Colliders)
            {
                if (!c.Collidable) continue;
                if (CollisionSystem.OverlapTest(this, c))
                {
                    Entity.X = tempX;
                    Entity.Y = tempY;
                    return c;
                }
            }

            Entity.X = tempX;
            Entity.Y = tempY;
            return null;
        }

        /// <summary>
        /// Checks for a collision with a list of tags.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The Collider that was hit.</returns>
        public Collider Collide(float x, float y, List<int> tags)
        {
            return Collide(x, y, tags.ToArray());
        }

        /// <summary>
        /// Checks for a collision with a list of tags.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The Collider that was hit.</returns>
        public Collider Collide(float x, float y, List<Enum> tags)
        {
            return Collide(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Checks for a collision with a list of tags and returns an Entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public Entity CollideEntity(float x, float y, params int[] tags)
        {
            var c = Collide(x, y, tags);
            if (c == null) return null;
            return c.Entity;
        }

        /// <summary>
        /// Checks for a collision with a list of tags and returns an Entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public T CollideEntity<T>(float x, float y, params int[] tags) where T : Entity
        {
            return (T)CollideEntity(x, y, tags);
        }

        /// <summary>
        /// Checks for a collision with a list of tags and returns an Entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public Entity CollideEntity(float x, float y, params Enum[] tags)
        {
            return CollideEntity(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Checks for a collision with a list of tags and returns an Entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public T CollideEntity<T>(float x, float y, params Enum[] tags) where T : Entity
        {
            return (T)CollideEntity(x, y, tags);
        }

        /// <summary>
        /// Checks for a collision with a specific Entity and returns an Entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="e">The Entity to check for.</param>
        /// <returns>The Entity that was hit.</returns>
        public Entity CollideEntity(float x, float y, Entity e)
        {
            return Collide(x, y, e).Entity;
        }

        /// <summary>
        /// Checks for a collision with a specific Entity and returns an Entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="e">The Entity to check for.</param>
        /// <returns>The Entity that was hit.</returns>
        public T CollideEntity<T>(float x, float y, Entity e) where T : Entity
        {
            return (T)CollideEntity(x, y, e);
        }

        /// <summary>
        /// Checks for a collision with a specific Collider and returns an Entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="c">The Collider to check for.</param>
        /// <returns>The Entity that was hit.</returns>
        public Entity CollideEntity(float x, float y, Collider c)
        {
            return Collide(x, y, c).Entity;
        }

        /// <summary>
        /// Checks for a collision with a specific Collider and returns an Entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="c">The Collider to check for.</param>
        /// <returns>The Entity that was hit.</returns>
        public T CollideEntity<T>(float x, float y, Collider c) where T : Entity
        {
            return (T)CollideEntity(x, y, c);
        }

        /// <summary>
        /// Checks for a collision with a list of tags and returns an Entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The list of tags to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public Entity CollideEntity(float x, float y, List<int> tags)
        {
            foreach (int t in tags)
            {
                Collider c;
                c = Collide(x, y, t);
                if (c != null)
                {
                    return c.Entity;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks for a collision with a list of tags and returns an Entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The list of tags to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public T CollideEntity<T>(float x, float y, List<int> tags) where T : Entity
        {
            return (T)CollideEntity(x, y, tags);
        }

        /// <summary>
        /// Checks for a collision with a list of Entities and returns an Entity.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="entities">The list of Entities to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public Entity CollideEntity(float x, float y, List<Entity> entities)
        {
            return Collide(x, y, entities)?.Entity;
        }

        /// <summary>
        /// Checks for a collision with a list of Entities and returns an Entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="entities">The list of Entities to check.</param>
        /// <returns>The Entity that was hit.</returns>
        public T CollideEntity<T>(float x, float y, List<T> entities) where T : Entity
        {
            return (T)CollideEntity(x, y, entities.ToList<Entity>()); // This blows up for some reason sometimes?
        }

        /// <summary>
        /// Creates a list of Colliders that this Collider is colliding with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of colliders.</returns>
        public List<Collider> CollideList(float x, float y, params int[] tags)
        {
            List<Collider> collided = [];
            if (Entity == null) return null;
            if (Entity.Scene == null) return null;

            float tempX = Entity.X, tempY = Entity.Y;
            Entity.X = x;
            Entity.Y = y;

            if (tags.Length == 0)
            {
                tags = [.. CollisionSystem.KnownColliderTags];
            }

            var values = new List<Collider>();
            CollisionSystem.ColliderQuadTree.FindCollisions(this, ref values);

            foreach (var c in values)
            {
                if (c.Entity != null)
                {
                    if (!c.Entity.Collidable) continue;
                    if (!c.Collidable) continue;
                    if (c.Entity == Entity) continue;
                }
                else
                {
                    continue;
                }
                foreach (int t in tags)
                {
                    if (c.HasTag(t))
                    {
                        if (CollisionSystem.OverlapTest(this, c))
                        {
                            Entity.X = tempX;
                            Entity.Y = tempY;
                            collided.Add(c);
                        }
                    }
                }
            }

            Entity.X = tempX;
            Entity.Y = tempY;

            return collided;
        }

        /// <summary>
        /// Creates a list of Colliders that this Collider is colliding with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of colliders.</returns>
        public List<Collider> CollideList(float x, float y, params Enum[] tags)
        {
            return CollideList(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Creates a list of Colliders that this Collider is colliding with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of colliders.</returns>
        public List<Collider> CollideList(float x, float y, List<Enum> tags)
        {
            return CollideList(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Creates a list of Colliders that this Collider is colliding with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of colliders.</returns>
        public List<Collider> CollideList(float x, float y, List<int> tags)
        {
            List<Collider> collided = [];
            List<Collider> c;

            foreach (int tag in tags)
            {
                c = CollideList(x, y, tag);
                foreach (Collider col in c)
                {
                    collided.Add(col);
                }
            }

            return collided;
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<Entity> CollideEntities(float x, float y, params int[] tags)
        {
            List<Entity> collided = [];

            List<Collider> clist = CollideList(x, y, tags);
            foreach (Collider c in clist)
            {
                if (!collided.Contains(c.Entity))
                {
                    if (c.Entity != Entity)
                    {
                        collided.Add(c.Entity);
                    }
                }
            }

            return collided;
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<Entity> CollideEntities(float x, float y, params Enum[] tags)
        {
            return CollideEntities(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<Entity> CollideEntities(float x, float y, List<Enum> tags)
        {
            return CollideEntities(x, y, Util.EnumToIntArray(tags));
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <typeparam name="T">The type of list to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<T> CollideEntities<T>(float x, float y, params int[] tags) where T : Entity
        {
            return CollideEntities(x, y, tags).Cast<T>().ToList();
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <typeparam name="T">The type of list to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<T> CollideEntities<T>(float x, float y, params Enum[] tags) where T : Entity
        {
            return CollideEntities(x, y, tags).Cast<T>().ToList();
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <typeparam name="T">The type of list to return.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<T> CollideEntities<T>(float x, float y, List<Enum> tags) where T : Entity
        {
            return CollideEntities(x, y, tags).Cast<T>().ToList();
        }

        /// <summary>
        /// Creates a list of Entities that the Collider has collided with.
        /// </summary>
        /// <typeparam name="T">The type of Entity.</typeparam>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="entities">The Entities to check.</param>
        /// <returns>A list of entities.</returns>
        public List<T> CollideEntities<T>(float x, float y, List<T> entities) where T : Entity
        {
            var list = new List<T>();

            foreach (var e in entities)
            {
                var c = Collide(x, y, e);
                if (c != null)
                {
                    if (c.Entity != null)
                    {
                        list.Add(e);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Creates a list of entities that the Collider has collided with.
        /// </summary>
        /// <param name="x">The x position to check.</param>
        /// <param name="y">The y position to check.</param>
        /// <param name="tags">The tags to check.</param>
        /// <returns>A list of entities.</returns>
        public List<Entity> CollideEntities(float x, float y, List<int> tags)
        {
            List<Entity> collided = [];
            List<Collider> c;

            foreach (int tag in tags)
            {
                c = CollideList(x, y, tag);
                foreach (Collider col in c)
                {
                    if (collided.Contains(col.Entity)) continue;
                    collided.Add(col.Entity);
                }
            }

            return collided;
        }

        /// <summary>
        /// Callback for when the Collider renders (for debugging purposes.)
        /// </summary>
        public abstract void DebugRenderIMGUI(Color? color = null);

        /// <summary>
        /// Add a new tag to the Collider.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        /// <returns>The Collider.</returns>
        public Collider AddTag(int tag)
        {
            if (Tags.Contains(tag)) return this;

            Tags.Add(tag);

            return this;
        }

        /// <summary>
        /// Add new tags to the Collider.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        /// <returns>The Collider.</returns>
        public Collider AddTag(params Enum[] tags)
        {
            AddTag(Util.EnumToIntArray(tags));

            return this;
        }

        public bool HasTag(params Enum[] tags)
        {
            return HasTag(Util.EnumToIntArray(tags));
        }

        public bool HasTag(params int[] tags)
        {
            foreach (var t in tags)
            {
                if (Tags.Contains(t)) return true;
            }
            return false;
        }

        /// <summary>
        /// Add new tags to the Collider.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        /// <returns>The Collider.</returns>
        public Collider AddTag(params int[] tags)
        {
            foreach (int t in tags)
            {
                AddTag(t);
            }

            return this;
        }

        /// <summary>
        /// Remove a tag from the Collider.
        /// </summary>
        /// <param name="tag">The tags to remove.</param>
        /// <returns>The Collider.</returns>
        public Collider RemoveTag(int tag)
        {
            Tags.Remove(tag);
            return this;
        }

        /// <summary>
        /// Remove tags from the Collider.
        /// </summary>
        /// <param name="tags">The tags to remove.</param>
        /// <returns>The Collider.</returns>
        public Collider RemoveTag(params int[] tags)
        {
            foreach (int t in tags)
            {
                RemoveTag(t);
            }

            return this;
        }

        /// <summary>
        /// Remove tags from the Collider.
        /// </summary>
        /// <param name="tags">The tags to remove.</param>
        /// <returns>The Collider.</returns>
        public Collider RemoveTag(params Enum[] tags)
        {
            RemoveTag(Util.EnumToIntArray(tags));

            return this;
        }

        /// <summary>
        /// Center the origin of the Collider.  Based off of Width and Height.
        /// </summary>
        public virtual void CenterOrigin()
        {
            OriginX = HalfWidth;
            OriginY = HalfHeight;
        }

        /// <summary>
        /// Set the positon of the Collider.
        /// </summary>
        /// <param name="x">The X position of the Collider.</param>
        /// <param name="y">The Y position of the Collider.</param>
        public void SetPosition(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Set the position of the Collider.
        /// </summary>
        /// <param name="xy">The Vector2 position of the Collider.</param>
        public void SetPosition(Vector2 xy)
        {
            SetPosition(xy.X, xy.Y);
        }

        /// <summary>
        /// Set the origin of the Collider 
        /// </summary>
        /// <param name="x">The X origin of the Collider.</param>
        /// <param name="y">The Y origin of the Collider.</param>
        public void SetOrigin(float x, float y)
        {
            OriginX = x;
            OriginY = y;
        }

        /// <summary>
        /// Set the origin of the Collider.
        /// </summary>
        /// <param name="xy">The Vector2 origin of the Collider.</param>
        public void SetOrigin(Vector2 xy)
        {
            SetOrigin(xy.X, xy.Y);
        }

        #endregion

    }
}
