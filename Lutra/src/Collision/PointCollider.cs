using Lutra.Utility;

namespace Lutra.Collision
{
    /// <summary>
    /// Point Collider.
    /// </summary>
    public class PointCollider : Collider
    {

        #region Constructors

        public PointCollider(int x, int y, params int[] tags)
        {
            Width = 1;
            Height = 1;
            X = x;
            Y = y;
            AddTag(tags);

        }

        public PointCollider(int x, int y, Enum tag, params Enum[] tags) : this(x, y)
        {
            AddTag(tag);
            AddTag(tags);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draw the collider for debug purposes.
        /// </summary>
        public override void DebugRenderIMGUI(Color? color = null)
        {
            if (color == null) color = Color.Red;

            if (Entity == null) return;

            ImGuiHelper.DrawRectangle(Left, Bottom, 1, 1, color.Value);
        }

        #endregion

    }
}
