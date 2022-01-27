using System.Numerics;
using System.Runtime.CompilerServices;
using Lutra.Utility;

namespace Lutra;

/// <summary>
/// A color stored in four 32-bit floating-point values, in RGBA component order.
/// Based on Veldrid's RgbaFloat with extra utility methods and easy conversion.
/// </summary>
public struct Color : IEquatable<Color>
{
    private readonly Vector4 _channels;

    /// <summary>
    /// The red component.
    /// </summary>
    public float R => _channels.X;
    /// <summary>
    /// The green component.
    /// </summary>
    public float G => _channels.Y;
    /// <summary>
    /// The blue component.
    /// </summary>
    public float B => _channels.Z;
    /// <summary>
    /// The alpha component.
    /// </summary>
    public float A => _channels.W;

    public byte ByteR => (byte)(_channels.X * 255);
    public byte ByteG => (byte)(_channels.Y * 255);
    public byte ByteB => (byte)(_channels.Z * 255);
    public byte ByteA => (byte)(_channels.W * 255);

    /// <summary>
    /// Get an 8 character hex string of the color (RRGGBBAA).
    /// </summary>
    public string ColorString
    {
        get => ByteR.ToString("X2") + ByteG.ToString("X2") + ByteB.ToString("X2") + ByteA.ToString("X2");
    }

    #region Constructors

    /// <summary>
    /// Constructs a new Color from the given float components.
    /// </summary>
    /// <param name="r">The red component as a float (0.0-1.0).</param>
    /// <param name="g">The green component as a float (0.0-1.0).</param>
    /// <param name="b">The blue component as a float (0.0-1.0).</param>
    /// <param name="a">The alpha component as a float (0.0-1.0).</param>
    public Color(float r, float g, float b, float a)
    {
        _channels = new Vector4(r, g, b, a);
    }

    /// <summary>
    /// Constructs a new Color from the given float components, with alpha set to 1.
    /// </summary>
    /// <param name="r">The red component as a float (0.0-1.0).</param>
    /// <param name="g">The green component as a float (0.0-1.0).</param>
    /// <param name="b">The blue component as a float (0.0-1.0).</param>
    public Color(float r, float g, float b)
    {
        _channels = new Vector4(r, g, b, 1);
    }

    /// <summary>
    /// Constructs a new Color from the XYZW components of a vector.
    /// </summary>
    /// <param name="channels">The vector containing the color components.</param>
    public Color(Vector4 channels)
    {
        _channels = channels;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new Color from the given byte components.
    /// </summary>
    /// <param name="r">The red component as a byte (0-255).</param>
    /// <param name="g">The green component as a byte (0-255).</param>
    /// <param name="b">The blue component as a byte (0-255).</param>
    /// <param name="a">The alpha component as a byte (0-255).</param>
    public static Color FromBytes(byte r, byte g, byte b, byte a)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    /// <summary>
    /// Creates a new color from a hexadecimal string.
    /// Formats are "RGB", "RGBA", "RRGGBB", and "RRGGBBAA".
    /// </summary>
    /// <param name="hex">A string with a hex representation of each channel.</param>
    public static Color FromString(string hex)
    {
        byte red = 255, green = 255, blue = 255, alpha = 255;

        if (hex.Length == 6)
        {
            red = Util.HexToByte(hex[0..2]);
            green = Util.HexToByte(hex[2..4]);
            blue = Util.HexToByte(hex[4..6]);
        }
        else if (hex.Length == 3)
        {
            red = Util.HexToByte(hex[0..1] + hex[0..1]);
            green = Util.HexToByte(hex[1..2] + hex[1..2]);
            blue = Util.HexToByte(hex[2..3] + hex[2..3]);
        }
        else if (hex.Length == 8)
        {
            red = Util.HexToByte(hex[0..2]);
            green = Util.HexToByte(hex[2..4]);
            blue = Util.HexToByte(hex[4..6]);
            alpha = Util.HexToByte(hex[6..8]);
        }
        else if (hex.Length == 4)
        {
            red = Util.HexToByte(hex[0..1] + hex[0..1]);
            green = Util.HexToByte(hex[1..2] + hex[1..2]);
            blue = Util.HexToByte(hex[2..3] + hex[2..3]);
            alpha = Util.HexToByte(hex[3..4] + hex[3..4]);
        }

        return Color.FromBytes(red, green, blue, alpha);
    }

    /// <summary>
    /// Create a shade of gray based on a value 0 to 1.
    /// </summary>
    /// <param name="rgb">The level of gray. 0 is black, 1 is white.</param>
    /// <returns>A color with RGB equal to the input value and alpha of 1.</returns>
    public static Color Shade(float rgb)
    {
        return new Color(rgb, rgb, rgb, 1);
    }

    #endregion

    #region Implementations & Overrides

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(R.GetHashCode(), G.GetHashCode(), B.GetHashCode(), A.GetHashCode());
    }

    /// <summary>
    /// Element-wise equality.
    /// </summary>
    /// <param name="other">The instance to compare to.</param>
    /// <returns>True if all elements are equal; false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Color other)
    {
        return _channels.Equals(other._channels);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        return obj is Color other && Equals(other);
    }

    /// <summary>
    /// Returns a string representation of this color.
    /// </summary>
    /// <returns>A string representation of this color.</returns>
    public override string ToString()
    {
        return string.Format("Color(R={0}, G={1}, B={2}, A={3})", R, G, B, A);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Converts this Color into a Vector4.
    /// </summary>
    /// <returns>A Vector4 containing the color channels.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 ToVector4()
    {
        return _channels;
    }

    /// <summary>
    /// Multiply the non-alpha channels by a scalar factor.
    /// </summary>
    /// <param name="colorFactor">The scalar factor.</param>
    public Color ColorMultiply(float colorFactor)
    {
        return new Color(R * colorFactor, G * colorFactor, B * colorFactor, A);
    }

    /// <summary>
    /// Multiply the alpha channel by a scalar factor.
    /// </summary>
    /// <param name="alphaFactor">The scalar factor.</param>
    public Color AlphaMultiply(float alphaFactor)
    {
        return new Color(R, G, B, A * alphaFactor);
    }

    /// <summary>
    /// Clone the color and set the red channel.
    /// </summary>
    /// <param name="red">The new red channel value.</param>
    public Color WithRed(float red)
    {
        return new Color(red, G, B, A);
    }

    /// <summary>
    /// Clone the color and set the green channel.
    /// </summary>
    /// <param name="green">The new green channel value.</param>
    public Color WithGreen(float green)
    {
        return new Color(R, green, B, A);
    }

    /// <summary>
    /// Clone the color and set the blue channel.
    /// </summary>
    /// <param name="blue">The new blue channel value.</param>
    public Color WithBlue(float blue)
    {
        return new Color(R, G, blue, A);
    }

    /// <summary>
    /// Clone the color and set the alpha channel.
    /// </summary>
    /// <param name="alpha">The new alpha channel value.</param>
    public Color WithAlpha(float alpha)
    {
        return new Color(R, G, B, alpha);
    }

    #endregion

    #region Operators

    /// <summary>
    /// Converts a Color to a Veldrid.RgbaFloat.
    /// </summary>
    /// <param name="color">The color.</param>
    public static implicit operator Veldrid.RgbaFloat(Color color)
    {
        return new Veldrid.RgbaFloat(color._channels);
    }

    /// <summary>
    /// Element-wise equality.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Element-wise inequality.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Element-wise addition.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator +(Color left, Color right)
    {
        return new Color(left._channels + right._channels);
    }

    /// <summary>
    /// Element-wise subtraction.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator -(Color left, Color right)
    {
        return new Color(left._channels - right._channels);
    }

    /// <summary>
    /// Element-wise multiplication.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator *(Color left, Color right)
    {
        return new Color(left._channels * right._channels);
    }

    /// <summary>
    /// Element-wise division.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator /(Color left, Color right)
    {
        return new Color(left._channels / right._channels);
    }

    #endregion

    #region Default Colors

    /// <summary>
    /// Clear (0, 0, 0, 0)
    /// </summary>
    public static readonly Color Clear = new Color(0, 0, 0, 0);
    public static readonly Color None = Clear;
    /// <summary>
    /// Black (0, 0, 0, 1)
    /// </summary>
    public static readonly Color Black = new Color(0, 0, 0, 1);
    /// <summary>
    /// White (1, 1, 1, 1)
    /// </summary>
    public static readonly Color White = new Color(1, 1, 1, 1);
    /// <summary>
    /// Red (1, 0, 0, 1)
    /// </summary>
    public static readonly Color Red = new Color(1, 0, 0, 1);
    /// <summary>
    /// Green (0, 1, 0, 1)
    /// </summary>
    public static readonly Color Green = new Color(0, 1, 0, 1);
    /// <summary>
    /// Blue (0, 0, 1, 1)
    /// </summary>
    public static readonly Color Blue = new Color(0, 0, 1, 1);
    /// <summary>
    /// Cyan (0, 1, 1, 1)
    /// </summary>
    public static readonly Color Cyan = new Color(0, 1, 1, 1);
    /// <summary>
    /// Magenta (1, 0, 1, 1)
    /// </summary>
    public static readonly Color Magenta = new Color(1, 0, 1, 1);
    /// <summary>
    /// Yellow (1, 1, 0, 1)
    /// </summary>
    public static readonly Color Yellow = new Color(1, 1, 0, 1);
    /// <summary>
    /// Grey (0.5f, 0.5f, 0.5f, 1)
    /// </summary>
    public static readonly Color Grey = new Color(.5f, .5f, .5f, 1);
    public static readonly Color Gray = Grey;
    /// <summary>
    /// Pink (1, 0.45f, 0.75f, 1)
    /// </summary>
    public static readonly Color Pink = new Color(1f, 0.45f, 0.75f, 1);
    /// <summary>
    /// Orange (1, 0.36f, 0, 1)
    /// </summary>
    public static readonly Color Orange = new Color(1f, 0.36f, 0f, 1);
    /// <summary>
    /// Cornflower Blue (0.3921f, 0.5843f, 0.9294f, 1)
    /// </summary>
    public static readonly Color CornflowerBlue = new Color(0.3921f, 0.5843f, 0.9294f, 1);
    /// <summary>
    /// Coral (1, 0.498f, 0.3137f, 1).
    /// </summary>
    public static readonly Color Coral = new Color(1, 0.498f, 0.3137f, 1);
    /// <summary>
    /// Coral Pink (1, 0.3137f, 0.498f, 1).
    /// </summary>
    public static readonly Color CoralPink = new Color(1, 0.3137f, 0.498f, 1);

    #endregion
}
