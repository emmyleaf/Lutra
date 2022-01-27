namespace Lutra.Input;

/// <summary>
/// Defines the axes on a Controller. Names are based on Xbox layout.
/// </summary>
public enum ControllerAxis : byte
{
    /// <summary>
    /// The X axis of the left stick.
    /// Range: -1.0 to 1.0
    /// </summary>
    LeftX = 0,

    /// <summary>
    /// The Y axis of the left stick.
    /// Range: -1.0 to 1.0
    /// </summary>
    LeftY = 1,

    /// <summary>
    /// The X axis of the right stick.
    /// Range: -1.0 to 1.0
    /// </summary>
    RightX = 2,

    /// <summary>
    /// The Y axis of the right stick.
    /// Range: -1.0 to 1.0
    /// </summary>
    RightY = 3,

    /// <summary>
    /// The left trigger.
    /// Range: 0.0 to 1.0
    /// </summary>
    LeftTrigger = 4,

    /// <summary>
    /// The right trigger.
    /// Range: 0.0 to 1.0
    /// </summary>
    RightTrigger = 5
}
