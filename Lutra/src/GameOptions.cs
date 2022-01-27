#nullable enable

using Veldrid;

namespace Lutra;

/// <summary>
/// Supplies initial values for a Game.
/// </summary>
public struct GameOptions
{
    /// <summary>
    /// The window title. Default: "Lutra"
    /// </summary>
    public string Title = "Lutra";

    /// <summary>
    /// The game's internal width. Default: 960
    /// </summary>
    public int Width = 960;

    /// <summary>
    /// The game's internal height. Default: 540
    /// </summary>
    public int Height = 540;

    /// <summary>
    /// Determines initial window size relative to the game's internal size. Default: 1
    /// </summary>
    public float ScaleXY = 1f;

    /// <summary>
    /// Maintain the original aspect ratio of the game when scaling the window. Default: true
    /// </summary>
    public bool LockAspectRatio = true;

    /// <summary>
    /// Maintain integer scaling of the game when scaling the window.
    /// Only applies when LockAspectRatio is also true. Default: true
    /// </summary>
    public bool LockIntegerScale = true;

    /// <summary>
    /// The target framerate. Default: 60.0
    /// </summary>
    public double TargetFrameRate = 60.0;

    /// <summary>
    /// When true, DeltaTime will be multiplied by 60. Default: false
    /// Useful when calculating values against the length of a 60fps frame, regardless of current framerate.
    /// </summary>
    public bool MeasureTimeInSixtiethSeconds = false;

    /// <summary>
    /// The preferred graphics backend for the Veldrid renderer. Default: null
    /// If set to null or an unsupported backend, Lutra will fall back to defaults.
    /// Preference order of defaults: Vulkan, Direct3D11, OpenGL, OpenGLES.
    /// NOTE: Metal backend currently has major rendering issues on my device (Early 2015 MacBook Pro).
    /// OpenGL has been successfully tested on all supported platforms (including macOS).
    /// Be aware of differences between backends: https://veldrid.dev/articles/backend-differences.html
    /// </summary>
    public GraphicsBackend? PreferredGraphicsBackend = null;
}
