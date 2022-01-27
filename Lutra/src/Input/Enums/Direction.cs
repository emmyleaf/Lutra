namespace Lutra.Input;

/// <summary>
/// Defines directions as combinable flags.
/// </summary>
[Flags]
public enum Direction
{
    None = 0,
    Up = 1 << 0,
    Right = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3,
    UpRight = Up | Right,
    UpLeft = Up | Left,
    DownRight = Down | Right,
    DownLeft = Down | Left
}
