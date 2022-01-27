namespace Lutra
{
    public class Component
    {
        public Entity Entity { get; internal set; }
        public Scene Scene => Entity?.Scene;
        public bool Active = true;
        private int order = 0;

        /// <summary>
        /// How long the Component has been alive (added to an Entity and updated.)
        /// </summary>
        public float Timer = 0;

        public int Order
        {
            get => order;
            set
            {
                order = value;
                Entity.Components.MarkUnsorted();
            }
        }

        #region Public Methods

        public void RemoveSelf()
        {
            Entity?.RemoveComponent(this);
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Called when the component is added to an entity.  The reference to the Entity is available here.
        /// </summary>
        public virtual void Added() { }

        /// <summary>
        /// Called when the component is removed from an entity.  The reference to Entity is now null.
        /// </summary>
        public virtual void Removed() { }

        /// <summary>
        /// Called first during the update.  Happens before Update.
        /// </summary>
        public virtual void UpdateFirst() { }

        /// <summary>
        /// Called during the update of the game.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Called last during the update.  Happens after Update.
        /// </summary>
        public virtual void UpdateLast() { }

        #endregion

        #region Delegates

        /// <summary>
        /// An action that fires just before the component's Added().
        /// </summary>
        public Action OnAdded = delegate { };

        /// <summary>
        /// An action that fires just before the component's Removed().
        /// </summary>
        public Action OnRemoved = delegate { };

        /// <summary>
        /// An action that fires just before the component's UpdateFirst().
        /// </summary>
        public Action OnUpdateFirst = delegate { };

        /// <summary>
        /// An action that fires just before the component's Update().
        /// </summary>
        public Action OnUpdate = delegate { };

        /// <summary>
        /// An action that is fired just before the component's UpdateLast().
        /// </summary>
        public Action OnUpdateLast = delegate { };

        #endregion

        #region Internal

        internal void InternalAdded(Entity entity)
        {
            Entity = entity;
            OnAdded();
            Added();
        }

        internal void InternalRemoved()
        {
            Entity = null;
            OnRemoved();
            Removed();
        }

        internal void InternalUpdateFirst()
        {
            if (!Active) return;
            OnUpdateFirst();
            UpdateFirst();
        }

        internal void InternalUpdate()
        {
            if (!Active) return;
            OnUpdate();
            Update();
        }

        internal void InternalUpdateLast()
        {
            if (!Active) return;
            OnUpdateLast();
            UpdateLast();
            Timer += Scene.Game.DeltaTime;
        }

        #endregion
    }
}
