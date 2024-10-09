using Lutra.Input;

namespace Lutra.Utility;

public static class InputHelper
{
    /// <summary>
    /// Get an array of the cardinal directions: Up, Right, down, Left.
    /// </summary>
    public static Direction[] CardinalDirections =>
        new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };

    /// <summary>
    /// Convert a char to a Key code.
    /// </summary>
    /// <param name="key">The char to convert into a Key.</param>
    /// <returns>The Key. Returns Key.Unknown if no match is found.</returns>
    public static Key CharToKey(char key)
    {
        return char.ToUpper(key) switch
        {
            'A' => Key.A,
            'B' => Key.B,
            'C' => Key.C,
            'D' => Key.D,
            'E' => Key.E,
            'F' => Key.F,
            'G' => Key.G,
            'H' => Key.H,
            'I' => Key.I,
            'J' => Key.J,
            'K' => Key.K,
            'L' => Key.L,
            'M' => Key.M,
            'N' => Key.N,
            'O' => Key.O,
            'P' => Key.P,
            'Q' => Key.Q,
            'R' => Key.R,
            'S' => Key.S,
            'T' => Key.T,
            'U' => Key.U,
            'V' => Key.V,
            'W' => Key.W,
            'X' => Key.X,
            'Y' => Key.Y,
            'Z' => Key.Z,
            '0' => Key.D0,
            '1' => Key.D1,
            '2' => Key.D2,
            '3' => Key.D3,
            '4' => Key.D4,
            '5' => Key.D5,
            '6' => Key.D6,
            '7' => Key.D7,
            '8' => Key.D8,
            '9' => Key.D9,
            '[' => Key.LeftBracket,
            ']' => Key.RightBracket,
            ';' => Key.Semicolon,
            ',' => Key.Comma,
            '.' => Key.Period,
            '/' => Key.Slash,
            '\\' => Key.Backslash,
            '#' => Key.NonUsHash,
            '=' => Key.Equals,
            ' ' => Key.Space,
            _ => Key.Unknown,
        };
    }
}
