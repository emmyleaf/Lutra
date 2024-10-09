namespace Lutra.Utility
{
    /// <summary>
    /// Class used to represent a range using a min and max.
    /// </summary>
    /// <remarks>
    /// Create a new Range.
    /// </remarks>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    public class Range(float min, float max)
    {

        #region Public Fields

        /// <summary>
        /// The minimum of the range.
        /// </summary>
        public float Min = min;

        /// <summary>
        /// The maximum of the range.
        /// </summary>
        public float Max = max;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get a random int from the range.  Floors the Min and Ceils the Max.
        /// </summary>
        /// <returns>A random int.</returns>
        public int RandInt => Rand.Int((int)Min, (int)Util.Ceil(Max));

        /// <summary>
        /// Get a random float from the range.
        /// </summary>
        /// <returns>A random float.</returns>
        public float RandFloat => Rand.Float(Min, Max);

        #endregion
        #region Constructors

        /// <summary>
        /// Create a new Range.
        /// </summary>
        /// <param name="max">Maximum value.  Minimum is -Maximum.</param>
        public Range(float max) : this(-max, max) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Test if this Range overlaps another Range.
        /// </summary>
        /// <param name="r">The Range to test against.</param>
        /// <returns>True if the ranges overlap.</returns>
        public bool Overlap(Range r)
        {
            if (r.Max < Min) return false;
            if (r.Min > Max) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Min, Max);
        }

        #endregion

    }
}
