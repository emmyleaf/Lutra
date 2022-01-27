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
        switch (char.ToUpper(key))
        {
            case 'A': return Key.A;
            case 'B': return Key.B;
            case 'C': return Key.C;
            case 'D': return Key.D;
            case 'E': return Key.E;
            case 'F': return Key.F;
            case 'G': return Key.G;
            case 'H': return Key.H;
            case 'I': return Key.I;
            case 'J': return Key.J;
            case 'K': return Key.K;
            case 'L': return Key.L;
            case 'M': return Key.M;
            case 'N': return Key.N;
            case 'O': return Key.O;
            case 'P': return Key.P;
            case 'Q': return Key.Q;
            case 'R': return Key.R;
            case 'S': return Key.S;
            case 'T': return Key.T;
            case 'U': return Key.U;
            case 'V': return Key.V;
            case 'W': return Key.W;
            case 'X': return Key.X;
            case 'Y': return Key.Y;
            case 'Z': return Key.Z;
            case '0': return Key.D0;
            case '1': return Key.D1;
            case '2': return Key.D2;
            case '3': return Key.D3;
            case '4': return Key.D4;
            case '5': return Key.D5;
            case '6': return Key.D6;
            case '7': return Key.D7;
            case '8': return Key.D8;
            case '9': return Key.D9;
            case '[': return Key.LeftBracket;
            case ']': return Key.RightBracket;
            case ';': return Key.Semicolon;
            case ',': return Key.Comma;
            case '.': return Key.Period;
            case '/': return Key.Slash;
            case '\\': return Key.Backslash;
            case '#': return Key.NonUsHash;
            case '=': return Key.Equals;
            case ' ': return Key.Space;
            default: return Key.Unknown;
        }
    }
}
