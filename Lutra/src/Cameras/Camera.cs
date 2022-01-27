using System.Numerics;
using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Cameras
{
    public class Camera
    {
        private float _x, _y, _width, _height, _angle, _scale;
        private Matrix4x4 _projection, _view;
        private RectFloat _bounds = new RectFloat();
        private bool _needsUpdate;

        #region Public Properties

        public float X
        {
            get => _x;
            set
            {
                _x = value;
                _needsUpdate = true;
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                _y = value;
                _needsUpdate = true;
            }
        }

        public float Width
        {
            get => _width;
            set
            {
                _width = value;
                _needsUpdate = true;
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                _height = value;
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

        public RectFloat Bounds
        {
            get
            {
                if (_needsUpdate) UpdateMatrices();
                return _bounds;
            }
        }

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

            _needsUpdate = true;
        }

        /// <summary>
        /// Create the default camera centred in a Game's bounds.
        /// </summary>
        public Camera(Game game) : this(game.Width / 2, game.Height / 2, game.Width, game.Height) { }

        #endregion

        #region Private Methods

        private void UpdateMatrices()
        {
            _bounds.X = _x - _width / 2;
            _bounds.Y = _y - _height / 2;
            _bounds.Width = _width;
            _bounds.Height = _height;

            _projection = VeldridResources.CreateOrthographicProjection(_width, _height);

            _view = Matrix4x4.CreateTranslation(-_x, -_y, 0) *
                Matrix4x4.CreateRotationZ(_angle * Util.DEG_TO_RAD) *
                Matrix4x4.CreateScale(_scale) *
                Matrix4x4.CreateTranslation(_width / 2, _height / 2, 0);

            _needsUpdate = false;
        }

        #endregion
    }
}
