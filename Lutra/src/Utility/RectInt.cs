// Based on code released under the MIT License
// Copyright (C) The Mono.Xna Team

using System.Numerics;
using System.Runtime.Serialization;

namespace Lutra.Utility
{
    /// <summary>
    /// Describes a 2D-rectangle with integer coordinates. 
    /// </summary>
    /// <remarks>
    /// Creates a new instance of <see cref="RectInt"/> struct, with the specified
    /// position, width, and height.
    /// </remarks>
    /// <param name="x">The x coordinate of the top-left corner of the created <see cref="RectInt"/>.</param>
    /// <param name="y">The y coordinate of the top-left corner of the created <see cref="RectInt"/>.</param>
    /// <param name="width">The width of the created <see cref="RectInt"/>.</param>
    /// <param name="height">The height of the created <see cref="RectInt"/>.</param>
    [DataContract]
    public struct RectInt(int x, int y, int width, int height) : IEquatable<RectInt>
    {
        #region Private Fields

        private static RectInt emptyRectangle = new();

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of the top-left corner of this <see cref="RectInt"/>.
        /// </summary>
        [DataMember]
        public int X = x;

        /// <summary>
        /// The y coordinate of the top-left corner of this <see cref="RectInt"/>.
        /// </summary>
        [DataMember]
        public int Y = y;

        /// <summary>
        /// The width of this <see cref="RectInt"/>.
        /// </summary>
        [DataMember]
        public int Width = width;

        /// <summary>
        /// The height of this <see cref="RectInt"/>.
        /// </summary>
        [DataMember]
        public int Height = height;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="RectInt"/> with X=0, Y=0, Width=0, Height=0.
        /// </summary>
        public static RectInt Empty => emptyRectangle;

        /// <summary>
        /// Returns the x coordinate of the left edge of this <see cref="RectInt"/>.
        /// </summary>
        public readonly int Left => X;

        /// <summary>
        /// Returns the x coordinate of the right edge of this <see cref="RectInt"/>.
        /// </summary>
        public readonly int Right => X + Width;

        /// <summary>
        /// Returns the y coordinate of the top edge of this <see cref="RectInt"/>.
        /// </summary>
        public readonly int Top => Y;

        /// <summary>
        /// Returns the y coordinate of the bottom edge of this <see cref="RectInt"/>.
        /// </summary>
        public readonly int Bottom => Y + Height;

        /// <summary>
        /// Whether or not this <see cref="RectInt"/> has a <see cref="Width"/> and
        /// <see cref="Height"/> of 0, and a <see cref="Location"/> of (0, 0).
        /// </summary>
        public readonly bool IsEmpty => (Width == 0) && (Height == 0) && (X == 0) && (Y == 0);

        /// <summary>
        /// The top-left coordinates of this <see cref="RectInt"/>.
        /// </summary>
        public readonly Vector2 Location => new(X, Y);

        /// <summary>
        /// The width-height coordinates of this <see cref="RectInt"/>.
        /// </summary>
        public readonly Vector2 Size => new(Width, Height);

        /// <summary>
        /// A <see cref="Vector2"/> located in the center of this <see cref="RectInt"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="Width"/> or <see cref="Height"/> is an odd number,
        /// the center point will be rounded down.
        /// </remarks>
        public readonly Vector2 Center => new(X + (Width / 2), Y + (Height / 2));

        #endregion
        #region Constructors

        #endregion

        #region Operators

        /// <summary>
        /// Compares whether two <see cref="RectInt"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="RectInt"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="RectInt"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(RectInt a, RectInt b)
        {
            return (a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height);
        }

        /// <summary>
        /// Compares whether two <see cref="RectInt"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="RectInt"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="RectInt"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(RectInt a, RectInt b)
        {
            return !(a == b);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectInt"/>; <c>false</c> otherwise.</returns>
        public readonly bool Contains(int x, int y)
        {
            return (X <= x) && (x < (X + Width)) && (Y <= y) && (y < (Y + Height));
        }

        /// <summary>
        /// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectInt"/>; <c>false</c> otherwise.</returns>
        public readonly bool Contains(float x, float y)
        {
            return (X <= x) && (x < (X + Width)) && (Y <= y) && (y < (Y + Height));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="RectInt"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectInt"/>; <c>false</c> otherwise.</returns>
        public readonly bool Contains(Vector2 value)
        {
            return (X <= value.X) && (value.X < (X + Width)) && (Y <= value.Y) && (value.Y < (Y + Height));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="RectInt"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectInt"/>; <c>false</c> otherwise. As an output parameter.</param>
        public readonly void Contains(ref Vector2 value, out bool result)
        {
            result = (X <= value.X) && (value.X < (X + Width)) && (Y <= value.Y) && (value.Y < (Y + Height));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="RectInt"/> lies within the bounds of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="value">The <see cref="RectInt"/> to check for inclusion in this <see cref="RectInt"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="RectInt"/>'s bounds lie entirely inside this <see cref="RectInt"/>; <c>false</c> otherwise.</returns>
        public readonly bool Contains(RectInt value)
        {
            return (X <= value.X) && ((value.X + value.Width) <= (X + Width)) && (Y <= value.Y) && ((value.Y + value.Height) <= (Y + Height));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="RectInt"/> lies within the bounds of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="value">The <see cref="RectInt"/> to check for inclusion in this <see cref="RectInt"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="RectInt"/>'s bounds lie entirely inside this <see cref="RectInt"/>; <c>false</c> otherwise. As an output parameter.</param>
        public readonly void Contains(ref RectInt value, out bool result)
        {
            result = (X <= value.X) && ((value.X + value.Width) <= (X + Width)) && (Y <= value.Y) && ((value.Y + value.Height) <= (Y + Height));
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override readonly bool Equals(object obj)
        {
            return (obj is RectInt other) && this == other;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="RectInt"/>.
        /// </summary>
        /// <param name="other">The <see cref="RectInt"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public readonly bool Equals(RectInt other)
        {
            return this == other;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="RectInt"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="RectInt"/>.</returns>
        public override readonly int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Width.GetHashCode();
                hash = hash * 23 + Height.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Adjusts the edges of this <see cref="RectInt"/> by specified horizontal and vertical amounts. 
        /// </summary>
        /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
        /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>
        /// Adjusts the edges of this <see cref="RectInt"/> by specified horizontal and vertical amounts. 
        /// </summary>
        /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
        /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= (int)horizontalAmount;
            Y -= (int)verticalAmount;
            Width += (int)horizontalAmount * 2;
            Height += (int)verticalAmount * 2;
        }

        /// <summary>
        /// Gets whether or not the other <see cref="RectInt"/> intersects with this rectangle.
        /// </summary>
        /// <param name="value">The other rectangle for testing.</param>
        /// <returns><c>true</c> if other <see cref="RectInt"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
        public readonly bool Intersects(RectInt value)
        {
            return value.Left < Right &&
                   Left < value.Right &&
                   value.Top < Bottom &&
                   Top < value.Bottom;
        }


        /// <summary>
        /// Gets whether or not the other <see cref="RectInt"/> intersects with this rectangle.
        /// </summary>
        /// <param name="value">The other rectangle for testing.</param>
        /// <param name="result"><c>true</c> if other <see cref="RectInt"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
        public readonly void Intersects(ref RectInt value, out bool result)
        {
            result = value.Left < Right &&
                     Left < value.Right &&
                     value.Top < Bottom &&
                     Top < value.Bottom;
        }

        /// <summary>
        /// Creates a new <see cref="RectInt"/> that contains overlapping region of two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectInt"/>.</param>
        /// <param name="value2">The second <see cref="RectInt"/>.</param>
        /// <returns>Overlapping region of the two rectangles.</returns>
        public static RectInt Intersect(RectInt value1, RectInt value2)
        {
            Intersect(ref value1, ref value2, out RectInt rectangle);
            return rectangle;
        }

        /// <summary>
        /// Creates a new <see cref="RectInt"/> that contains overlapping region of two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectInt"/>.</param>
        /// <param name="value2">The second <see cref="RectInt"/>.</param>
        /// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
        public static void Intersect(ref RectInt value1, ref RectInt value2, out RectInt result)
        {
            if (value1.Intersects(value2))
            {
                int right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                int left_side = Math.Max(value1.X, value2.X);
                int top_side = Math.Max(value1.Y, value2.Y);
                int bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new RectInt(left_side, top_side, right_side - left_side, bottom_side - top_side);
            }
            else
            {
                result = new RectInt(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="offsetX">The x coordinate to add to this <see cref="RectInt"/>.</param>
        /// <param name="offsetY">The y coordinate to add to this <see cref="RectInt"/>.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="offsetX">The x coordinate to add to this <see cref="RectInt"/>.</param>
        /// <param name="offsetY">The y coordinate to add to this <see cref="RectInt"/>.</param>
        public void Offset(float offsetX, float offsetY)
        {
            X += (int)offsetX;
            Y += (int)offsetY;
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="RectInt"/>.
        /// </summary>
        /// <param name="amount">The x and y components to add to this <see cref="RectInt"/>.</param>
        public void Offset(Vector2 amount)
        {
            X += (int)amount.X;
            Y += (int)amount.Y;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="RectInt"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="RectInt"/>.</returns>
        public override readonly string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
        }

        /// <summary>
        /// Creates a new <see cref="RectInt"/> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectInt"/>.</param>
        /// <param name="value2">The second <see cref="RectInt"/>.</param>
        /// <returns>The union of the two rectangles.</returns>
        public static RectInt Union(RectInt value1, RectInt value2)
        {
            int x = Math.Min(value1.X, value2.X);
            int y = Math.Min(value1.Y, value2.Y);
            return new RectInt(x, y,
                                 Math.Max(value1.Right, value2.Right) - x,
                                     Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        /// <summary>
        /// Creates a new <see cref="RectInt"/> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectInt"/>.</param>
        /// <param name="value2">The second <see cref="RectInt"/>.</param>
        /// <param name="result">The union of the two rectangles as an output parameter.</param>
        public static void Union(ref RectInt value1, ref RectInt value2, out RectInt result)
        {
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        }

        /// <summary>
        /// Deconstruction method for <see cref="RectInt"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public readonly void Deconstruct(out int x, out int y, out int width, out int height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        #endregion
    }
}
