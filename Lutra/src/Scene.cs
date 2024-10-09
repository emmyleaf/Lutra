using System.Collections.Generic;
using System.Linq;
using Lutra.Cameras;
using Lutra.Collision;
using Lutra.Utility;
using Lutra.Utility.Collections;
using Lutra.Utility.Glide;

namespace Lutra
{
    public class Scene
    {
        private readonly EntityList _entityList;
        private readonly GraphicList _graphicList;

        #region Public

        public Game Game { get; internal set; }

        public IManagedList<Entity> Entities => _entityList;
        public IManagedList<Graphic> Graphics => _graphicList;

        public bool Visible = true;

        /// <summary>
        /// The current time since this Scene has started.
        /// </summary>
        public float Timer;

        /// <summary>
        /// When true, this Scene adds a Camera to the CameraManager's stack on Begin.
        /// On End, the scene removes this Camera and any Cameras above it on the stack.
        /// </summary>
        public bool ManagesOwnCamera = true;

        /// <summary>
        /// When ManagesOwnCamera is true, this Camera will be non-null between the Scene's Begin and End calls. 
        /// </summary>
        public Camera MainCamera { get; private set; }

        /// <summary>
        /// The tween manager that controls tweens in this scene.
        /// </summary>
        public Tweener Tweener = new();

        public Scene()
        {
            _entityList = new(this);
            _graphicList = [];
        }

        /// <summary>
        /// Get a list of entities that can be cast to the given type parameter.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to search for.</typeparam>
        /// <returns>A list of all entities of type TEntity in the scene.</returns>
        public List<TEntity> GetEntities<TEntity>()
        where TEntity : Entity
        {
            var list = new List<TEntity>();
            foreach (var entity in _entityList)
            {
                if (entity is TEntity tEntity)
                {
                    list.Add(tEntity);
                }
            }
            return list;
        }

        /// <summary>
        /// Get the first instance of an Entity of type TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to search for.</typeparam>
        /// <returns>The first entity of type TEntity in the scene, or null.</returns>
        public TEntity GetEntity<TEntity>() where TEntity : Entity
        {
            foreach (var entity in _entityList)
            {
                if (entity is TEntity tEntity)
                {
                    return tEntity;
                }
            }
            return null;
        }

        /// <summary>
        /// Get a list of entities that have a component that can be cast to the given type parameter.
        /// WARNING: This could be very slow!
        /// </summary>
        public List<Entity> GetEntitiesWith<TComponent>()
        where TComponent : Component
        {
            var list = new List<Entity>();
            foreach (var entity in _entityList)
            {
                foreach (var component in entity.Components)
                {
                    if (component is TComponent)
                    {
                        list.Add(entity);
                        break;
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get all entities that have a collider with the given collision tag.
        /// WARNING: This could be very slow!
        /// </summary>
        public static IEnumerable<Entity> GetEntitiesWithCollider(int colliderTag)
        {
            return CollisionSystem.Instance.RegisteredColliders
                .Where(c => c.Tags.Contains(colliderTag))
                .Select(c => c.Entity);
        }

        /// <summary>
        /// Get all entities that have a collider with the given collision tag.
        /// WARNING: This could be very slow!
        /// </summary>
        public static IEnumerable<Entity> GetEntitiesWithCollider(Enum colliderTag)
        {
            return GetEntitiesWithCollider(Convert.ToInt32(colliderTag));
        }

        public void Add(Entity entity)
        {
            _entityList.Add(entity);
        }

        public void Remove(Entity entity)
        {
            _entityList.Remove(entity);
        }

        public void Add(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public void Remove(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public void Add(params Entity[] entities)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                Add(entities[i]);
            }
        }

        public void Remove(params Entity[] entities)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                Remove(entities[i]);
            }
        }

        public void AddGraphic(Graphic graphic)
        {
            graphic.Scene = this;
            _graphicList.Add(graphic);
        }

        public void RemoveGraphic(Graphic graphic)
        {
            graphic.Scene = null;
            _graphicList.Remove(graphic);
        }

        public void RemoveAllGraphics()
        {
            while (_graphicList.Count > 0)
            {
                RemoveGraphic(_graphicList[^1]);
            }
        }

        /// <summary>
        /// Tweens a set of numeric properties on an object.
        /// </summary>
        /// <param name="target">The object to tween.</param>
        /// <param name="values">The values to tween to, in an anonymous type ( new { prop1 = 100, prop2 = 0} ).</param>
        /// <param name="duration">Duration of the tween in seconds.</param>
        /// <param name="delay">Delay before the tween starts, in seconds.</param>
        /// <returns>The tween created, for setting properties on.</returns>
        public Tween Tween(object target, object values, float duration, float delay = 0)
        {
            return Tweener.Tween(target, values, duration, delay);
        }

        /// <summary>
        /// Pause a group of entities.
        /// </summary>
        /// <param name="group">The group to pause.</param>
        // TODO: Implement this!
        public void PauseGroup(int group)
        {
            throw new NotImplementedException();
            // if (groupsToUnpause.Contains(group))
            // {
            //     groupsToUnpause.Remove(group);
            // }
            // else
            // {
            //     groupsToPause.Add(group);
            // }
        }

        /// <summary>
        /// Resume a paused group of entities.
        /// </summary>
        /// <param name="group">The group to resume.</param>
        public void ResumeGroup(int group)
        {
            throw new NotImplementedException();
            // if (!IsGroupPaused(group)) return;

            // if (groupsToPause.Contains(group))
            // {
            //     groupsToPause.Remove(group);
            // }
            // else
            // {
            //     groupsToUnpause.Add(group);
            // }
        }

        /// <summary>
        /// Update internal lists of entities and graphics.
        /// This includes processing additions/removals and sorting.
        /// </summary>
        public void UpdateLists()
        {
            _entityList.UpdateLists();
            _graphicList.UpdateLists();
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Called when the scene begins after being switched to, or added to the stack.
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// Called when the scene ends after being switched away from, or removed from the stack.
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Called when the scene is paused because a new scene is stacked on it.
        /// </summary>
        public virtual void Pause() { }

        /// <summary>
        /// Called when the scene resumes after a scene is added above it.
        /// </summary>
        public virtual void Resume() { }

        /// <summary>
        /// The first update of the scene.
        /// </summary>
        public virtual void UpdateFirst() { }

        /// <summary>
        /// The main update loop of the scene.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// The last update of the scene.
        /// </summary>
        public virtual void UpdateLast() { }

        /// <summary>
        /// Contains custom render logic for the scene.
        /// Entities/Graphics added to the scene render before this.
        /// </summary>
        public virtual void Render() { }

        #endregion

        #region Delegates

        /// <summary>
        /// An action that fires just before the scene's Begin().
        /// </summary>
        public Action OnBegin = delegate { };

        /// <summary>
        /// An action that fires just before the scene's End().
        /// </summary>
        public Action OnEnd = delegate { };

        /// <summary>
        /// An action that fires just before the scene's Pause().
        /// </summary>
        public Action OnPause = delegate { };

        /// <summary>
        /// An action that fires just before the scene's Resume().
        /// </summary>
        public Action OnResume = delegate { };

        /// <summary>
        /// An action that fires just before the scene's UpdateFirst().
        /// </summary>
        public Action OnUpdateFirst = delegate { };

        /// <summary>
        /// An action that fires just before the scene's Update().
        /// </summary>
        public Action OnUpdate = delegate { };

        /// <summary>
        /// An action that fires just before the scene's UpdateLast().
        /// </summary>
        public Action OnUpdateLast = delegate { };

        /// <summary>
        /// An action that fires just before the scene's Render().
        /// </summary>
        public Action OnRender = delegate { };

        /// <summary>
        /// An action that fires when the scene is drawing to ImGui.
        /// </summary>
        public Action IMGUIDraw = delegate { };

        #endregion

        #region Internal

        internal void InternalBegin(Game game)
        {
            Game = game;

            if (ManagesOwnCamera)
            {
                MainCamera = new Camera(game);
                Game.CameraManager.PushCamera(MainCamera);
            }

            OnBegin();
            Begin();
        }

        internal void InternalEnd()
        {
            OnEnd();
            End();

            _entityList.End();

            if (ManagesOwnCamera)
            {
                Game.CameraManager.RemoveCameraAndAbove(MainCamera);
                MainCamera = null;
            }

            Game = null;
        }

        internal void InternalPause()
        {
            OnPause();
            Pause();
        }

        internal void InternalResume()
        {
            OnResume();
            Resume();
        }

        internal void InternalUpdate()
        {
            _entityList.UpdateLists();
            foreach (var entity in _entityList) entity.InternalUpdateFirst();
            OnUpdateFirst();
            UpdateFirst();

            Tweener.Update(Game.DeltaTime);

            _entityList.UpdateLists();
            foreach (var entity in _entityList) entity.InternalUpdate();
            OnUpdate();
            Update();

            _entityList.UpdateLists();
            foreach (var entity in _entityList) entity.InternalUpdateLast();
            OnUpdateLast();
            UpdateLast();

            Timer += Game.DeltaTime;
        }

        internal void InternalRender()
        {
            if (Game != null && Visible)
            {
                _graphicList.UpdateLists();
                _graphicList.RenderAll();
                OnRender();
                Render();
            }
        }

        #endregion

        #region DEPRECATED - Otter Compatibility

        /// <summary>
        /// The X position of the top left of MainCamera. Only use if ManagesOwnCamera is true.
        /// Note: this exists for Otter compatibility, so does not refer to the camera focal point.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public float CameraX
        {
            get => MainCamera?.Left ?? default;
            set
            {
                if (MainCamera != null) MainCamera.X = value + (MainCamera.Width / 2);
            }
        }

        /// <summary>
        /// The Y position of the top left of MainCamera. Only use if ManagesOwnCamera is true.
        /// Note: this exists for Otter compatibility, so does not refer to the camera focal point.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public float CameraY
        {
            get => MainCamera?.Top ?? default;
            set
            {
                if (MainCamera != null) MainCamera.Y = value + (MainCamera.Height / 2);
            }
        }

        /// <summary>
        /// The zoom level (scale) of MainCamera. Only use if ManagesOwnCamera is true.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public float CameraZoom
        {
            get => MainCamera?.Scale ?? default;
            set
            {
                if (MainCamera != null) MainCamera.Scale = value;
            }
        }

        /// <summary>
        /// The angle of MainCamera. Only use if ManagesOwnCamera is true.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public float CameraAngle
        {
            get => MainCamera?.Angle ?? default;
            set
            {
                if (MainCamera != null) MainCamera.Angle = value;
            }
        }

        /// <summary>
        /// The X position of the focal point of MainCamera. Only use if ManagesOwnCamera is true.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public float CameraCenterX => MainCamera?.X ?? default;

        /// <summary>
        /// The Y position of the focal point of MainCamera. Only use if ManagesOwnCamera is true.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public float CameraCenterY => MainCamera?.Y ?? default;

        /// <summary>
        /// Set the focal point of MainCamera. Only use if ManagesOwnCamera is true.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public void CenterCamera(float x, float y)
        {
            MainCamera.X = x;
            MainCamera.Y = y;
        }

        /// <summary>
        /// The bounds that the camera should be clamped inside.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        // TODO: Implement this!
        public RectInt CameraBounds;

        /// <summary>
        /// Determines if the camera will be clamped inside the CameraBounds rectangle.
        /// </summary>
        [Obsolete("Deprecated. Use MainCamera directly or CameraManager instead.")]
        public bool UseCameraBounds = false;

        #endregion
    }
}
