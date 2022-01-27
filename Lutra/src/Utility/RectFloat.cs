// Based on code released under the MIT License
// Copyright (C) The Mono.Xna Team

using System.Numerics;
using System.Runtime.Serialization;

namespace Lutra.Utility
{
    /// <summary>
    /// Describes a 2D-rectangle with float coordinates. 
    /// </summary>
    [DataContract]
    public struct RectFloat : IEquatable<RectFloat>
    {
        #region Private Fields

        private static RectFloat emptyRectangle = new RectFloat();

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of the top-left corner of this <see cref="RectFloat"/>.
        /// </summary>
        [DataMember]
        public float X;

        /// <summary>
        /// The y coordinate of the top-left corner of this <see cref="RectFloat"/>.
        /// </summary>
        [DataMember]
        public float Y;

        /// <summary>
        /// The width of this <see cref="RectFloat"/>.
        /// </summary>
        [DataMember]
        public float Width;

        /// <summary>
        /// The height of this <see cref="RectFloat"/>.
        /// </summary>
        [DataMember]
        public float Height;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a <see cref="RectFloat"/> with X=0, Y=0, Width=0, Height=0.
        /// </summary>
        public static RectFloat Empty
        {
            get { return emptyRectangle; }
        }

        /// <summary>
        /// Returns the x coordinate of the left edge of this <see cref="RectFloat"/>.
        /// </summary>
        public float Left
        {
            get { return this.X; }
        }

        /// <summary>
        /// Returns the x coordinate of the right edge of this <see cref="RectFloat"/>.
        /// </summary>
        public float Right
        {
            get { return (this.X + this.Width); }
        }

        /// <summary>
        /// Returns the y coordinate of the top edge of this <see cref="RectFloat"/>.
        /// </summary>
        public float Top
        {
            get { return this.Y; }
        }

        /// <summary>
        /// Returns the y coordinate of the bottom edge of this <see cref="RectFloat"/>.
        /// </summary>
        public float Bottom
        {
            get { return (this.Y + this.Height); }
        }

        /// <summary>
        /// Whether or not this <see cref="RectFloat"/> has a <see cref="Width"/> and
        /// <see cref="Height"/> of 0, and a <see cref="Location"/> of (0, 0).
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        /// <summary>
        /// The top-left coordinates of this <see cref="RectFloat"/>.
        /// </summary>
        public Vector2 Location => new Vector2(this.X, this.Y);

        /// <summary>
        /// The width-height coordinates of this <see cref="RectFloat"/>.
        /// </summary>
        public Vector2 Size => new Vector2(this.Width, this.Height);

        /// <summary>
        /// A <see cref="Vector2"/> located in the center of this <see cref="RectFloat"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="Width"/> or <see cref="Height"/> is an odd number,
        /// the center point will be rounded down.
        /// </remarks>
        public Vector2 Center => new Vector2(this.X + (this.Width / 2), this.Y + (this.Height / 2));

        /// <summary>
        /// The top-left coordinates of this <see cref="RectFloat"/>.
        /// </summary>
        public Vector2 TopLeft => new Vector2(this.X, this.Y);

        /// <summary>
        /// The top-right coordinates of this <see cref="RectFloat"/>.
        /// </summary>
        public Vector2 TopRight => new Vector2(this.X + this.Width, this.Y);

        /// <summary>
        /// The bottom-left coordinates of this <see cref="RectFloat"/>.
        /// </summary>
        public Vector2 BottomLeft => new Vector2(this.X, this.Y + this.Height);

        /// <summary>
        /// The bottom-right coordinates of this <see cref="RectFloat"/>.
        /// </summary>
        public Vector2 BottomRight => new Vector2(this.X + this.Width, this.Y + this.Height);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RectFloat"/> struct, with the specified
        /// position, width, and height.
        /// </summary>
        /// <param name="x">The x coordinate of the top-left corner of the created <see cref="RectFloat"/>.</param>
        /// <param name="y">The y coordinate of the top-left corner of the created <see cref="RectFloat"/>.</param>
        /// <param name="width">The width of the created <see cref="RectFloat"/>.</param>
        /// <param name="height">The height of the created <see cref="RectFloat"/>.</param>
        public RectFloat(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compares whether two <see cref="RectFloat"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="RectFloat"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="RectFloat"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(RectFloat a, RectFloat b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

        /// <summary>
        /// Compares whether two <see cref="RectFloat"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="RectFloat"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="RectFloat"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(RectFloat a, RectFloat b)
        {
            return !(a == b);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="x">The x coordinate of the point to check for containment.</param>
        /// <param name="y">The y coordinate of the point to check for containment.</param>
        /// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectFloat"/>; <c>false</c> otherwise.</returns>
        public bool Contains(float x, float y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="RectFloat"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectFloat"/>; <c>false</c> otherwise.</returns>
        public bool Contains(Vector2 value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="value">The coordinates to check for inclusion in this <see cref="RectFloat"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectFloat"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Contains(ref Vector2 value, out bool result)
        {
            result = ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="RectFloat"/> lies within the bounds of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="value">The <see cref="RectFloat"/> to check for inclusion in this <see cref="RectFloat"/>.</param>
        /// <returns><c>true</c> if the provided <see cref="RectFloat"/>'s bounds lie entirely inside this <see cref="RectFloat"/>; <c>false</c> otherwise.</returns>
        public bool Contains(RectFloat value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        /// <summary>
        /// Gets whether or not the provided <see cref="RectFloat"/> lies within the bounds of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="value">The <see cref="RectFloat"/> to check for inclusion in this <see cref="RectFloat"/>.</param>
        /// <param name="result"><c>true</c> if the provided <see cref="RectFloat"/>'s bounds lie entirely inside this <see cref="RectFloat"/>; <c>false</c> otherwise. As an output parameter.</param>
        public void Contains(ref RectFloat value, out bool result)
        {
            result = ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RectFloat) && this == ((RectFloat)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="other">The <see cref="RectFloat"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(RectFloat other)
        {
            return this == other;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="RectFloat"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="RectFloat"/>.</returns>
        public override int GetHashCode()
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
        /// Adjusts the edges of this <see cref="RectFloat"/> by specified horizontal and vertical amounts. 
        /// </summary>
        /// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
        /// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>
        /// Gets whether or not the other <see cref="RectFloat"/> intersects with this rectangle.
        /// </summary>
        /// <param name="value">The other rectangle for testing.</param>
        /// <returns><c>true</c> if other <see cref="RectFloat"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
        public bool Intersects(RectFloat value)
        {
            return value.Left < Right &&
                   Left < value.Right &&
                   value.Top < Bottom &&
                   Top < value.Bottom;
        }


        /// <summary>
        /// Gets whether or not the other <see cref="RectFloat"/> intersects with this rectangle.
        /// </summary>
        /// <param name="value">The other rectangle for testing.</param>
        /// <param name="result"><c>true</c> if other <see cref="RectFloat"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
        public void Intersects(ref RectFloat value, out bool result)
        {
            result = value.Left < Right &&
                     Left < value.Right &&
                     value.Top < Bottom &&
                     Top < value.Bottom;
        }

        /// <summary>
        /// Creates a new <see cref="RectFloat"/> that contains overlapping region of two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectFloat"/>.</param>
        /// <param name="value2">The second <see cref="RectFloat"/>.</param>
        /// <returns>Overlapping region of the two rectangles.</returns>
        public static RectFloat Intersect(RectFloat value1, RectFloat value2)
        {
            RectFloat rectangle;
            Intersect(ref value1, ref value2, out rectangle);
            return rectangle;
        }

        /// <summary>
        /// Creates a new <see cref="RectFloat"/> that contains overlapping region of two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectFloat"/>.</param>
        /// <param name="value2">The second <see cref="RectFloat"/>.</param>
        /// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
        public static void Intersect(ref RectFloat value1, ref RectFloat value2, out RectFloat result)
        {
            if (value1.Intersects(value2))
            {
                float right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                float left_side = Math.Max(value1.X, value2.X);
                float top_side = Math.Max(value1.Y, value2.Y);
                float bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new RectFloat(left_side, top_side, right_side - left_side, bottom_side - top_side);
            }
            else
            {
                result = new RectFloat(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="offsetX">The x coordinate to add to this <see cref="RectFloat"/>.</param>
        /// <param name="offsetY">The y coordinate to add to this <see cref="RectFloat"/>.</param>
        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>
        /// Changes the <see cref="Location"/> of this <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="amount">The x and y components to add to this <see cref="RectFloat"/>.</param>
        public void Offset(Vector2 amount)
        {
            X += amount.X;
            Y += amount.Y;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="RectFloat"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="RectFloat"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
        }

        /// <summary>
        /// Creates a new <see cref="RectFloat"/> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectFloat"/>.</param>
        /// <param name="value2">The second <see cref="RectFloat"/>.</param>
        /// <returns>The union of the two rectangles.</returns>
        public static RectFloat Union(RectFloat value1, RectFloat value2)
        {
            float x = Math.Min(value1.X, value2.X);
            float y = Math.Min(value1.Y, value2.Y);
            return new RectFloat(x, y,
                                 Math.Max(value1.Right, value2.Right) - x,
                                     Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        /// <summary>
        /// Creates a new <see cref="RectFloat"/> that completely contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first <see cref="RectFloat"/>.</param>
        /// <param name="value2">The second <see cref="RectFloat"/>.</param>
        /// <param name="result">The union of the two rectangles as an output parameter.</param>
        public static void Union(ref RectFloat value1, ref RectFloat value2, out RectFloat result)
        {
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        }

        /// <summary>
        /// Deconstruction method for <see cref="RectFloat"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Deconstruct(out float x, out float y, out float width, out float height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        #endregion
    }
}
