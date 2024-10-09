using Lutra.Utility;

namespace Lutra.Collision
{
    /// <summary>
    /// Circle Collider.
    /// </summary>
    public class CircleCollider : Collider
    {

        #region Public Fields

        /// <summary>
        /// The radius of the circle.
        /// </summary>
        public float Radius;

        #endregion

        #region Public Properties

        public override float Width => Radius * 2;

        public override float Height => Radius * 2;

        #endregion

        #region Constructors

        public CircleCollider(float radius, params int[] tags)
        {
            Radius = radius;
            AddTag(tags);
        }

        public CircleCollider(float radius, Enum tag, params Enum[] tags) : this(radius)
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

            var camera = Entity.Scene.Game.CameraManager.ActiveCamera;
            var x = (CenterX - camera.Left) * 2f;
            var y = (CenterY - camera.Top) * 2f;

            ImGuiHelper.DrawCircle(x, y, (int)Math.Round(2f * Radius) - 1, color.Value);
        }

        #endregion

    }
}
