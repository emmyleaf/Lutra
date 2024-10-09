using System.Collections.Generic;
using System.Numerics;
using Lutra.Collision;
using Lutra.Graphics;
using Lutra.Utility.Collections;
using Lutra.Utility.Glide;

namespace Lutra
{
    public class Entity
    {
        private readonly ComponentList components;
        private readonly LinkedHashSet<Collider> colliders;
        private readonly LinkedHashSet<Graphic> graphics;

        private int order = 0;
        internal int _deprecated_layer = 0;
        internal Surface _deprecated_surface = null;

        public IManagedList<Component> Components => components;
        public IReadOnlyList<Collider> Colliders => colliders;
        public IReadOnlyList<Graphic> Graphics => graphics;

        /// <summary>
        /// The Scene that controls and updates this entity.
        /// </summary>
        public Scene Scene { get; internal set; }

        public Transform Transform { get; protected set; }

        public bool Active = true;

        /// <summary>
        /// The name of this entity.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Determines if the Entity will render.
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// Determines if the Entity will collide with other entities. The entity can still check for
        /// collisions, but will not register as a collision with other entities.
        /// </summary>
        public bool Collidable = true;

        /// <summary>
        /// How long the Entity has been active.
        /// </summary>
        public float Timer;

        /// <summary>
        /// How long the entity should live in the scene before removing itself. If this is set the
        /// entity will be automatically removed when the Timer exceeds this value.
        /// </summary>
        public float LifeSpan;

        /// <summary>
        /// The tween manager that controls tweens on this entity.
        /// </summary>
        private readonly Tweener Tweener = new();

        /// <summary>
        /// The pause group this entity is a part of.
        /// </summary>
        // TODO: Implement
        public int Group;

        /// <summary>
        /// Deprecated.
        /// The fallback Layer for Graphics attached to this Entity. Defaults to 0.
        /// Even with this property set, Layer can be overridden on individual Graphics.
        /// </summary>
        [Obsolete("Deprecated. Please set Layer on Graphics individually instead.", false)]
        public int Layer
        {
            get => _deprecated_layer;
            set
            {
                _deprecated_layer = value;
                Scene?.Graphics.MarkUnsorted();
            }
        }

        /// <summary>
        /// Deprecated.
        /// The fallback Surface for Graphics attached to this Entity. Defaults to null.
        /// Even with this property set, Surface can be overridden on individual Graphics.
        /// </summary>
        [Obsolete("Deprecated. Please set Surface on Graphics individually instead.", false)]
        public Surface Surface
        {
            get => _deprecated_surface;
            set => _deprecated_surface = value;
        }

        public int Order
        {
            get => order;
            set
            {
                order = value;
                Scene?.Entities.MarkUnsorted();
            }
        }

        public float X
        {
            get => Transform.X;
            set => Transform.X = value;
        }

        public float Y
        {
            get => Transform.Y;
            set => Transform.Y = value;
        }

        public Vector2 Position
        {
            get => new(Transform.X, Transform.Y);
            set { Transform.X = value.X; Transform.Y = value.Y; }
        }

        public Collider Collider
        {
            get => colliders.Last();
            set
            {
                if (colliders.Count > 0)
                {
                    RemoveCollider(colliders.Last());
                }
                AddCollider(value);
            }
        }

        public Graphic Graphic
        {
            get => graphics.Last();
            set
            {
                if (graphics.Count > 0)
                {
                    RemoveGraphic(graphics.Last());
                }
                AddGraphic(value);
            }
        }

        public Entity(float x = 0, float y = 0)
        {
            Transform = new();
            components = new(this);
            colliders = [];
            graphics = [];
            X = x;
            Y = y;
        }

        #region Public Methods

        public void RemoveSelf()
        {
            Scene?.Remove(this);
        }

        public void AddComponent(Component component)
        {
            components.Add(component);
        }

        public void RemoveComponent(Component component)
        {
            components.Remove(component);
        }

        public void AddComponents(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                AddComponent(component);
            }
        }

        public void RemoveComponents(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                RemoveComponent(component);
            }
        }

        public void AddComponents(params Component[] components)
        {
            for (int i = 0; i < components.Length; i++)
            {
                AddComponent(components[i]);
            }
        }

        public void RemoveComponents(params Component[] components)
        {
            for (int i = 0; i < components.Length; i++)
            {
                RemoveComponent(components[i]);
            }
        }

        public void AddCollider(Collider collider)
        {
            if (colliders.Add(collider))
            {
                OnColliderAdded(collider);
                collider.Entity = this;

                if (Scene != null)
                {
                    CollisionSystem.Instance.RegisterCollider(collider);
                }
            }
        }

        public void RemoveCollider(Collider collider)
        {
            if (colliders.Remove(collider))
            {
                OnColliderRemoved(collider);
                collider.Entity = null;
                CollisionSystem.Instance.UnregisterCollider(collider);
            }
        }

        public void AddGraphic(Graphic graphic)
        {
            if (graphics.Add(graphic))
            {
                graphic.Entity = this;
                Scene?.AddGraphic(graphic);
            }
        }

        public void RemoveGraphic(Graphic graphic)
        {
            if (graphics.Remove(graphic))
            {
                graphic.Entity = null;
                Scene?.RemoveGraphic(graphic);
            }
        }

        public void ClearGraphics()
        {
            foreach (var graphic in graphics)
            {
                graphic.Entity = null;
                Scene?.RemoveGraphic(graphic);
            }
            graphics.Clear();
        }

        /// <summary>
        /// Get a reference to the entity's current Scene cast to a type that extends Scene.
        /// </summary>
        public TScene GetScene<TScene>()
        where TScene : Scene
        {
            return Scene as TScene;
        }

        /// <summary>
        /// Get the first Component of the given Type.
        /// </summary>
        public TComponent GetComponent<TComponent>()
        where TComponent : Component
        {
            foreach (var component in components)
            {
                if (component is TComponent tComponent) return tComponent;
            }

            return null;
        }

        /// <summary>
        /// Get a list of all components of the given Type.
        /// </summary>
        public List<TComponent> GetComponents<TComponent>()
        where TComponent : Component
        {
            var list = new List<TComponent>();

            foreach (var component in components)
            {
                if (component is TComponent tComponent) list.Add(tComponent);
            }

            return list;
        }

        /// <summary>
        /// Tweens a set of numeric properties on an object.
        /// </summary>
        /// <param name="target">The object to tween.</param>
        /// <param name="values">The values to tween to, in an anonymous type ( new { prop1 = 100, prop2 = 0} ).</param>
        /// <param name="duration">Duration of the tween in seconds.</param>
        /// <param name="delay">Delay before the tween starts, in seconds.</param>
        /// <returns>The tween created, for setting properties on.</returns>
        [Obsolete("Deprecated. Use Tween on Scene instead.")]
        public Tween Tween(object target, object values, float duration, float delay = 0)
        {
            return Tweener.Tween(target, values, duration, delay);
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Called when the scene updates after the entity is added to a scene. The reference to the Scene is available here.
        /// </summary>
        public virtual void Added() { }

        /// <summary>
        /// Called when the entity is removed from a scene or when the scene ends. The reference to Scene will be null after this.
        /// </summary>
        public virtual void Removed() { }

        /// <summary>
        /// Called first during the update. Happens before Update.
        /// </summary>
        public virtual void UpdateFirst() { }

        /// <summary>
        /// Called during the update of the game.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Called last during the update. Happens after Update.
        /// </summary>
        public virtual void UpdateLast() { }

        #endregion

        #region Delegates

        /// <summary>
        /// An action that fires just before the entity's Added().
        /// </summary>
        public Action OnAdded = delegate { };

        /// <summary>
        /// An action that fires just before the entity's Removed().
        /// </summary>
        public Action OnRemoved = delegate { };

        /// <summary>
        /// An action that fires just before the entity's UpdateFirst().
        /// </summary>
        public Action OnUpdateFirst = delegate { };

        /// <summary>
        /// An action that fires just before the entity's Update().
        /// </summary>
        public Action OnUpdate = delegate { };

        /// <summary>
        /// An action that is fired just before the entity's UpdateLast().
        /// </summary>
        public Action OnUpdateLast = delegate { };

        /// <summary>
        /// An action that is fired when a collider is added to the entity.
        /// </summary>
        public Action<Collider> OnColliderAdded = delegate { };

        /// <summary>
        /// An action that is fired when a collider is removed from the entity.
        /// </summary>
        public Action<Collider> OnColliderRemoved = delegate { };

        #endregion

        #region Internal

        internal void InternalAdded(Scene scene)
        {
            Scene = scene;
            OnAdded();
            Added();

            foreach (var graphic in graphics)
            {
                Scene.AddGraphic(graphic);
            }

            foreach (var collider in colliders)
            {
                CollisionSystem.Instance.RegisterCollider(collider);
            }
        }

        internal void InternalRemoved()
        {
            OnRemoved();
            Removed();

            foreach (var graphic in graphics)
            {
                Scene.RemoveGraphic(graphic);
            }

            foreach (var collider in colliders)
            {
                CollisionSystem.Instance.UnregisterCollider(collider);
            }

            Scene = null;
        }

        internal void InternalUpdateFirst()
        {
            if (!Active) return;
            components.UpdateLists();
            foreach (var component in components) component.InternalUpdateFirst();
            OnUpdateFirst();
            UpdateFirst();
        }

        internal void InternalUpdate()
        {
            if (!Active) return;
            Tweener.Update(Scene.Game.DeltaTime);
            components.UpdateLists();
            foreach (var component in components) component.InternalUpdate();
            OnUpdate();
            Update();
        }

        internal void InternalUpdateLast()
        {
            if (!Active) return;
            components.UpdateLists();
            foreach (var component in components) component.InternalUpdateLast();
            OnUpdateLast();
            UpdateLast();

            Timer += Scene.Game.DeltaTime;
            if (LifeSpan > 0)
            {
                if (Timer >= LifeSpan)
                {
                    RemoveSelf();
                }
            }
        }

        #endregion
    }
}
