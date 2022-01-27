using Lutra.Utility;

namespace Lutra.Collision
{
    /// <summary>
    /// Rectangle Collider.
    /// </summary>
    public class BoxCollider : Collider
    {
        #region Constructors

        /// <summary>
        /// Creates a new box collider.
        /// </summary>
        /// <param name="width">The width of the collider.</param>
        /// <param name="height">The height of the collider.</param>
        /// <param name="tags">Any tags the collider should have.</param>
        public BoxCollider(int width, int height, params int[] tags)
        {
            Width = width;
            Height = height;
            AddTag(tags);
        }

        public BoxCollider(int width, int height, Enum tag, params Enum[] tags) : this(width, height)
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

            // TODO: sort out debug rendering for all parameters, now that we have cameras & scaling!

            var camera = Entity.Scene.Game.CameraManager.ActiveCamera;
            var left = (Left - camera.Left) * 2f;
            var top = (Top - camera.Top) * 2f;
            var width = Width * 2f;
            var height = Height * 2f;

            ImGuiHelper.DrawRectangle(left, top, width, height, color.Value);
        }

        #endregion

    }
}
