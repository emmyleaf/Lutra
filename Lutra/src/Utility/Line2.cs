using System.Numerics;

namespace Lutra.Utility
{
    /// <summary>
    /// Class for a simple line with two points.
    /// </summary>
    public class Line2
    {

        #region Public Fields

        /// <summary>
        /// The X position for the first point.
        /// </summary>
        public float X1;

        /// <summary>
        /// The Y position for the first point.
        /// </summary>
        public float Y1;

        /// <summary>
        /// The X position for the second point.
        /// </summary>
        public float X2;

        /// <summary>
        /// The Y position for the second point.
        /// </summary>
        public float Y2;

        #endregion

        #region Public Properties

        /// <summary>
        /// The first point of the line as a vector2.
        /// </summary>
        public Vector2 PointA
        {
            get => new(X1, Y1);
            set { X1 = value.X; Y1 = value.Y; }
        }

        /// <summary>
        /// The second point of a line as a vector2.
        /// </summary>
        public Vector2 PointB
        {
            get => new(X2, Y2);
            set { X2 = value.X; Y2 = value.Y; }
        }

        /// <summary>
        /// A in the line equation Ax + By = C.
        /// </summary>
        public float A => Y2 - Y1;

        /// <summary>
        /// B in the line equation Ax + By = C.
        /// </summary>
        public float B => X1 - X2;

        /// <summary>
        /// C in the line equation Ax + By = C.
        /// </summary>
        public float C => A * X1 + B * Y1;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Line2.
        /// </summary>
        /// <param name="x1">X of the first point</param>
        /// <param name="y1">Y of the first point</param>
        /// <param name="x2">X of the second point</param>
        /// <param name="y2">Y of the second point</param>
        public Line2(float x1, float y1, float x2, float y2)
        {
            X1 = x1; X2 = x2; Y1 = y1; Y2 = y2;
        }

        /// <summary>
        /// Create a new Line2.
        /// </summary>
        /// <param name="xy1">X,Y of the first point</param>
        /// <param name="xy2">X,Y of the second point</param>
        public Line2(Vector2 xy1, Vector2 xy2)
        {
            PointA = xy1; PointB = xy2;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Intersection test on another line. (http://ideone.com/PnPJgb)
        /// </summary>
        /// <param name="other">The line to test against</param>
        /// <returns></returns>
        public bool Intersects(Line2 other)
        {
            //A = X1, Y1; B = X2, Y2; C = other.X1, other.Y1; D = other.X2, other.Y2;
            Vector2 A = new(X1, Y1);
            Vector2 B = new(X2, Y2);
            Vector2 C = new(other.X1, other.Y1);
            Vector2 D = new(other.X2, other.Y2);

            Vector2 CmP = new(C.X - A.X, C.Y - A.Y);
            Vector2 r = new(B.X - A.X, B.Y - A.Y);
            Vector2 s = new(D.X - C.X, D.Y - C.Y);

            float CmPxr = CmP.X * r.Y - CmP.Y * r.X;
            float CmPxs = CmP.X * s.Y - CmP.Y * s.X;
            float rxs = r.X * s.Y - r.Y * s.X;

            if (CmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((C.X - A.X < 0f) != (C.X - B.X < 0f))
                        || ((C.Y - A.Y < 0f) != (C.Y - B.Y < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }

        public (float, float) IntersectTU(Line2 other)
        {
            //A = X1, Y1; B = X2, Y2; C = other.X1, other.Y1; D = other.X2, other.Y2;
            Vector2 A = new(X1, Y1);
            Vector2 B = new(X2, Y2);
            Vector2 C = new(other.X1, other.Y1);
            Vector2 D = new(other.X2, other.Y2);

            Vector2 CmP = new(C.X - A.X, C.Y - A.Y);
            Vector2 r = new(B.X - A.X, B.Y - A.Y);
            Vector2 s = new(D.X - C.X, D.Y - C.Y);

            float CmPxr = CmP.X * r.Y - CmP.Y * r.X;
            float CmPxs = CmP.X * s.Y - CmP.Y * s.X;
            float rxs = r.X * s.Y - r.Y * s.X;

            if (rxs == 0f)
                return (-1.0f, -1.0f);

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t, u);
        }

        public override string ToString()
        {
            return "{X1: " + X1 + " Y1: " + Y1 + " X2: " + X2 + " Y2: " + Y2 + "}";
        }

        /// <summary>
        /// Check intersection against a rectangle.
        /// </summary>
        /// <param name="x">X Position of the rectangle.</param>
        /// <param name="y">Y Position of the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <returns>True if the line intersects any line on the rectangle, or if the line is inside the rectangle.</returns>
        public bool IntersectsRect(float x, float y, float width, float height)
        {
            if (Util.InRect(X1, Y1, x, y, width, height)) return true;
            if (Util.InRect(X2, Y2, x, y, width, height)) return true;
            if (Intersects(new Line2(x, y, x + width, y))) return true;
            if (Intersects(new Line2(x + width, y, x + width, y + height))) return true;
            if (Intersects(new Line2(x + width, y + height, x, y + height))) return true;
            if (Intersects(new Line2(x, y + height, x, y))) return true;

            return false;
        }

        /// <summary>
        /// Check the intersection against a circle.
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public bool IntersectCircle(Vector2 circle, float radius)
        {
            // find the closest point on the line segment to the center of the circle
            Vector2 line = PointB - PointA;
            float lineLength = (float)line.Length();
            Vector2 lineNorm = (1 / lineLength) * line;
            Vector2 segmentToCircle = circle - PointA;
            float closestPointOnSegment = (float)Vector2.Dot(segmentToCircle, line) / lineLength;

            // Special cases where the closest point happens to be the end points
            Vector2 closest;
            if (closestPointOnSegment < 0) closest = PointA;
            else if (closestPointOnSegment > lineLength) closest = PointB;
            else closest = PointA + closestPointOnSegment * lineNorm;

            // Find that distance.  If it is less than the radius, then we 
            // are within the circle
            var distanceFromClosest = circle - closest;
            var distanceFromClosestLength = distanceFromClosest.Length();
            if (distanceFromClosestLength > radius) return false;

            return true;
        }

        #endregion

    }
}
