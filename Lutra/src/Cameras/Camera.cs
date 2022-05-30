using System.Numerics;
using Lutra.Graphics;
using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Cameras
{
    public class Camera
    {
        private float _x, _y, _width, _height, _angle, _scale;
        private Matrix4x4 _projection, _view;
        private RectFloat _bounds;
        private bool _needsUpdate;

        #region Public Properties

        public float X
        {
            get => _x;
            set
            {
                _x = value;
                _bounds.X = _x - _width / 2f;
                _needsUpdate = true;
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                _y = value;
                _bounds.Y = _y - _height / 2f;
                _needsUpdate = true;
            }
        }

        public float Width
        {
            get => _width;
            set
            {
                _width = value;
                _bounds.Width = _width;
                _bounds.X = _x - _width / 2f;
                _needsUpdate = true;
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                _bounds.Height = _height;
                _bounds.Y = _y - _height / 2f;
                _needsUpdate = true;
            }
        }

        public float Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                _needsUpdate = true;
            }
        }

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _needsUpdate = true;
            }
        }

        public float Left => Bounds.Left;
        public float Right => Bounds.Right;
        public float Bottom => Bounds.Bottom;
        public float Top => Bounds.Top;

        public RectFloat Bounds => _bounds;

        public Matrix4x4 Projection
        {
            get
            {
                if (_needsUpdate) UpdateMatrices();
                return _projection;
            }
        }

        public Matrix4x4 View
        {
            get
            {
                if (_needsUpdate) UpdateMatrices();
                return _view;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a camera. X and Y represent the camera's focus point, the center of its bounds.
        /// </summary>
        public Camera(float x, float y, float width, float height, float angle = 0.0f, float scale = 1.0f)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _angle = angle;
            _scale = scale;

            _bounds = new RectFloat(x - width / 2f, y - height / 2f, width, height);

            _needsUpdate = true;
        }

        /// <summary>
        /// Create a default camera centred in a Game's bounds.
        /// </summary>
        public Camera(Game game) : this(game.HalfWidth, game.HalfHeight, game.Width, game.Height) { }

        /// <summary>
        /// Create a default camera centred in a Surface's bounds.
        /// </summary>
        public Camera(Surface surface) : this(surface.HalfWidth, surface.HalfHeight, surface.Width, surface.Height) { }

        #endregion

        #region Private Methods

        private void UpdateMatrices()
        {
            _projection = VeldridResources.CreateOrthographicProjection(_width, _height);

            _view = Matrix4x4.CreateTranslation(-_x, -_y, 0f) *
                Matrix4x4.CreateRotationZ(_angle * Util.DEG_TO_RAD) *
                Matrix4x4.CreateScale(_scale, _scale, 1f) *
                Matrix4x4.CreateTranslation(_width / 2f, _height / 2f, 0f);

            _needsUpdate = false;
        }

        #endregion
    }
}
