using System.Collections.Generic;
using System.Numerics;
using Lutra.Cameras;
using Lutra.Graphics;
using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra
{
    /// <summary>
    /// Base abstract class used for anything that can be rendered.
    /// </summary>
    public abstract class Graphic
    {
        private RectInt _textureRegion;
        private bool _relative = true;
        private bool _fixed = false;
        private int? layer = null;
        private Color color = Color.White;

        #region Public Properties

        public Scene Scene { get; internal set; }
        public Entity Entity { get; internal set; }

        public Transform Transform { get; protected set; } = new();

        public LutraTexture Texture { get; protected set; }

        /// <summary>
        /// Defines the area of the Texture to be rendered.
        /// </summary>
        public RectInt TextureRegion
        {
            get
            {
                return _textureRegion;
            }
            set
            {
                _textureRegion = value;
                NeedsUpdate = true;
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                NeedsUpdate = true;
            }
        }

        public bool Visible = true;

        public Action OnRender;

        /// <summary>
        /// The name of this Graphic.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The alpha value of the graphic's Color.
        /// </summary>
        public float Alpha
        {
            get => Color.A;
            set => Color = Color.WithAlpha(value);
        }

        /// <summary>
        /// Determines if the Graphic is rendered relative to its Entity.
        /// </summary>
        public bool Relative
        {
            get => _relative;
            set
            {
                _relative = value;
                NeedsUpdate = true;
            }
        }
        
        /// <summary>
        /// Determines if the Graphic, when inheriting transforms, should snap to the integer grid.
        /// </summary>
        public bool RelativeIsIntegerFixed
        {
            get => _fixed;
            set
            {
                _fixed = value;
                NeedsUpdate = true;
            }
        }

        /// <summary>
        /// The Layer (render order) of the graphic. Lower values render on top of higher values.
        /// If not set, Layer falls back to the Entity's Layer value for Otter compatibility.
        /// If not attached to an Entity, defaults to 0.
        /// </summary>
        public int Layer
        {
            get => layer ?? Entity?._deprecated_layer ?? 0;
            set
            {
                layer = value;
                Scene?.Graphics.MarkUnsorted();
            }
        }

        /// <summary>
        /// The list of Surfaces the Graphic should draw to.
        /// If this list is empty, this falls back to the Surface property of the Entity for Otter compatibility.
        /// If there is not Enitity Surface, the default behaviour is to render to the main game Surface.
        /// </summary>
        public List<Surface> Surfaces { get; } = new();

        /// <summary>
        /// The width of the Graphic.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the Graphic.
        /// </summary>
        public int Height;

        /// <summary>
        /// Determines if the graphic's matrices/drawables will have to be updated before it's rendered.
        /// Default value is true, ensuring we update on the first render loop.
        /// </summary>
        protected internal bool NeedsUpdate = true;

        #endregion

        #region Computed Properties

        /// <summary>
        /// The X position of the Graphic.
        /// </summary>
        public float X
        {
            get => Transform.X;
            set => Transform.X = value;
        }

        /// <summary>
        /// The Y position of the Graphic.
        /// </summary>
        public float Y
        {
            get => Transform.Y;
            set => Transform.Y = value;
        }

        /// <summary>
        /// The position of the Graphic.
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(Transform.X, Transform.Y); }
            set { Transform.X = value.X; Transform.Y = value.Y; }
        }

        /// <summary>
        /// The scroll factor for the x position. Used for parallax like effects.
        /// Values lower than 1 will scroll slower than the camera (appear to be further away). 
        /// Values higher than 1 will scroll faster than the camera (appear to be closer).
        /// </summary>
        public float ScrollX
        {
            get => Transform.ScrollX;
            set => Transform.ScrollX = value;
        }

        /// <summary>
        /// The scroll factor for the y position. Used for parallax like effects.
        /// Values lower than 1 will scroll slower than the camera (appear to be further away).
        /// Values higher than 1 will scroll faster than the camera (appear to be closer).
        /// </summary>
        public float ScrollY
        {
            get => Transform.ScrollY;
            set => Transform.ScrollY = value;
        }

        /// <summary>
        /// Set both ScrollX and ScrollY.
        /// </summary>
        public float Scroll
        {
            set { Transform.ScrollX = value; Transform.ScrollY = value; }
        }

        /// <summary>
        /// The horizontal scale of the graphic.
        /// </summary>
        public float ScaleX
        {
            get => Transform.ScaleX;
            set => Transform.ScaleX = value;
        }

        /// <summary>
        /// The vertical scale of the graphic.
        /// </summary>
        public float ScaleY
        {
            get => Transform.ScaleY;
            set => Transform.ScaleY = value;
        }

        /// <summary>
        /// Sets both the ScaleX and ScaleY at the same time.
        /// </summary>
        public float Scale
        {
            set { Transform.ScaleX = value; Transform.ScaleY = value; }
        }

        /// <summary>
        /// The angle of rotation of the graphic.
        /// </summary>
        public float Angle
        {
            get => Transform.Rotation;
            set => Transform.Rotation = value;
        }

        /// <summary>
        /// The X origin point to scale and rotate the graphic with.
        /// </summary>
        public float OriginX
        {
            get => Transform.OriginX;
            set => Transform.OriginX = value;
        }

        /// <summary>
        /// The Y origin point to scale and rotate the graphic with.
        /// </summary>
        public float OriginY
        {
            get => Transform.OriginY;
            set => Transform.OriginY = value;
        }

        /// <summary>
        /// The origin point to scale and rotate the graphic with.
        /// </summary>
        public Vector2 Origin
        {
            get => new Vector2(Transform.OriginX, Transform.OriginY);
            set { Transform.OriginX = value.X; Transform.OriginY = value.Y; }
        }

        /// <summary>
        /// The width in pixels of the image after applying ScaleX.
        /// </summary>
        public float ScaledWidth
        {
            get => Width * Transform.ScaleX;
            set => Transform.ScaleX = value / Width;
        }

        /// <summary>
        /// The height in pixels of the image after applying ScaleY.
        /// </summary>
        public float ScaledHeight
        {
            get => Height * Transform.ScaleY;
            set => Transform.ScaleY = value / Height;
        }

        // public Vector2 ScaledBounds
        // {
        //     get => new Vector2(ScaledWidth, ScaledHeight);
        //     set { ScaledWidth = value.X; ScaledHeight = value.Y; }
        // }

        // public Vector2 Center => new Vector2(HalfWidth, HalfHeight);

        // public Vector2 Bounds
        // {
        //     get => new Vector2(Width, Height);
        //     set { Width = value.X; Height = value.Y; }
        // }

        /// <summary>
        /// Half of the width.
        /// </summary>
        public float HalfWidth => Width / 2f;

        /// <summary>
        /// Half of the height.
        /// </summary>
        public float HalfHeight => Height / 2f;

        /// <summary>
        /// The horizontal amount to randomly offset the graphic by each frame.
        /// TODO: Implement shake!
        /// </summary>
        public float ShakeX;

        /// <summary>
        /// The vertial amount to randomly offset the graphic by each frame.
        /// </summary>
        public float ShakeY;

        /// <summary>
        /// A shortcut to set both ShakeX and ShakeY.
        /// </summary>
        public float Shake
        {
            set { ShakeX = value; ShakeY = value; }
        }

        public Matrix4x4 WorldMatrix
        {
            get
            {
                if (Relative && Entity != null)
                {
                    if (RelativeIsIntegerFixed)
                    {
                        var mat = Transform.Matrix * Entity.Transform.Matrix;
                        // Floor all position elements of matrix to affix this to the grid..
                        mat.M41 = MathF.Floor(mat.M41);
                        mat.M42 = MathF.Floor(mat.M42);
                        mat.M43 = MathF.Floor(mat.M43);
                        mat.M44 = MathF.Floor(mat.M44);
                        return mat;
                    }
                    return Transform.Matrix * Entity.Transform.Matrix;
                }

                return Transform.Matrix;
            }
        }

        #endregion

        /// <summary>
        /// Set the Texture that the Graphic is using (if it is using one.)
        /// If TextureRegion is empty, set it to the whole size of the new Texture.
        /// </summary>
        /// <param name="texture">The LutraTexture to use.</param>
        public virtual void SetTexture(LutraTexture texture)
        {
            Texture = texture;
            NeedsUpdate = true;

            if (TextureRegion.IsEmpty)
            {
                TextureRegion = new RectInt(0, 0, (int)Texture.Width, (int)Texture.Height);
            }
        }

        public void CenterOrigin()
        {
            Transform.OriginX = (int)HalfWidth;
            Transform.OriginY = (int)HalfHeight;
        }

        /// <summary>
        /// Add a surface that the Graphic should render to.
        /// </summary>
        public void AddSurface(Surface target)
        {
            Surfaces.Add(target);
        }

        /// <summary>
        /// Remove a surface from the list of surfaces that the Graphic should render to.
        /// </summary>
        public void RemoveSurface(Surface target)
        {
            Surfaces.Remove(target);
        }

        /// <summary>
        /// Remove all Surfaces from the list of Surfaces that the Graphic should render to.
        /// </summary>
        public void ClearSurfaces()
        {
            Surfaces.Clear();
        }

        protected internal abstract void Render();

        internal void InternalRender()
        {
            // Do not render if we are invisible or part of an invisible entity
            if (!Visible) return;
            if (Entity != null && !Entity.Visible) return;

            OnRender?.Invoke();

            // Draw to each surface in Surfaces (or to Entity's Surface, or to current Draw Target)
            if (Surfaces.Count > 0)
            {
                Surface temp = Draw.Target;
                foreach (var surface in Surfaces)
                {
                    Draw.Target = surface;
                    if (!surface.UseSceneCamera)
                    {
                        Game.Instance.CameraManager.PushCamera(new Camera(surface));
                    }
                    Render();
                    if (!surface.UseSceneCamera)
                    {
                        Game.Instance.CameraManager.PopCamera();
                    }
                }
                Draw.Target = temp;
            }
            else if (Entity?._deprecated_surface != null)
            {
                Surface temp = Draw.Target;
                Draw.Target = Entity._deprecated_surface;
                if (!Entity._deprecated_surface.UseSceneCamera)
                {
                    Game.Instance.CameraManager.PushCamera(new Camera(Entity._deprecated_surface));
                }
                Render();
                if (!Entity._deprecated_surface.UseSceneCamera)
                {
                    Game.Instance.CameraManager.PopCamera();
                }
                Draw.Target = temp;
            }
            else
            {
                Render();
            }

            // Reset if we need an update AFTER subclasses may have used it
            NeedsUpdate = false;
        }
    }
}
