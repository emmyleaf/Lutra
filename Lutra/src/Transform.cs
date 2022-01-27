using System.Numerics;
using Lutra.Utility;

namespace Lutra;

/// <summary>
/// Represents a set of transforms as a cached Matrix4x4 that can be passed to shaders.
/// </summary>
public class Transform
{
    private float _x = 0, _y = 0, _scaleX = 1, _scaleY = 1, _rotation = 0, _originX = 0, _originY = 0;
    private float _scrollX = 1, _scrollY = 1;
    private Matrix4x4 _matrix;
    private bool _needsUpdate = true;

    public bool WillBeUpdated => _needsUpdate || _scrollX != 1 || _scrollY != 1;
    public bool Overridden { get; private set; } = false;

    /// <summary>
    /// The X position. Initial value: 0
    /// </summary>
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The Y position. Initial value: 0
    /// </summary>
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The horizontal scale. Initial value: 1
    /// </summary>
    public float ScaleX
    {
        get => _scaleX;
        set
        {
            _scaleX = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The vertical scale. Initial value: 1
    /// </summary>
    public float ScaleY
    {
        get => _scaleY;
        set
        {
            _scaleY = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The angle of rotation in degrees. Initial value: 0
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The X origin point to scale and rotate with. Initial value: 0
    /// </summary>
    public float OriginX
    {
        get => _originX;
        set
        {
            _originX = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The Y origin point to scale and rotate with. Initial value: 0
    /// </summary>
    public float OriginY
    {
        get => _originY;
        set
        {
            _originY = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The scroll factor for the x position. Used for parallax like effects.
    /// Values lower than 1 will scroll slower than the camera (appear to be further away). 
    /// Values higher than 1 will scroll faster than the camera (appear to be closer).
    /// Initial value: 1
    /// </summary>
    public float ScrollX
    {
        get => _scrollX;
        set
        {
            _scrollX = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The scroll factor for the y position. Used for parallax like effects.
    /// Values lower than 1 will scroll slower than the camera (appear to be further away).
    /// Values higher than 1 will scroll faster than the camera (appear to be closer).
    /// Initial value: 1
    /// </summary>
    public float ScrollY
    {
        get => _scrollY;
        set
        {
            _scrollY = value;
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// The transform Matrix.
    /// If the setter is used, the matrix will no longer be updated and will be manually controlled from then on.
    /// </summary>
    public Matrix4x4 Matrix
    {
        get
        {
            if (!Overridden && WillBeUpdated) UpdateMatrix();
            return _matrix;
        }
        set
        {
            _matrix = value;
            Overridden = true;
        }
    }

    private void UpdateMatrix()
    {
        var camera = Game.Instance.CameraManager.ActiveCamera;
        var renderX = _x + camera.Left * (1 - _scrollX);
        var renderY = _y + camera.Top * (1 - _scrollY);

        _matrix = Matrix4x4.CreateTranslation(-_originX, -_originY, 0) *
                  Matrix4x4.CreateRotationZ(-_rotation * Util.DEG_TO_RAD) *
                  Matrix4x4.CreateScale(_scaleX, _scaleY, 1) *
                  Matrix4x4.CreateTranslation(renderX, renderY, 0);

        _needsUpdate = false;
    }
}
