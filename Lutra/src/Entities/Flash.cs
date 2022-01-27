using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Entities
{
    /// <summary>
    /// Entity that acts as a screen flash.
    /// </summary>
    public class Flash : Entity
    {

        #region Static Fields

        /// <summary>
        /// The default life span for all created Flash Entities.
        /// </summary>
        public static int DefaultLifeSpan = 60;

        #endregion

        #region Private Fields

        private Shape shape;

        #endregion

        #region Public Fields

        /// <summary>
        /// The Color for the Flash.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The initial alpha for the Flash.
        /// </summary>
        public float Alpha = 1;

        /// <summary>
        /// The final alpha for the Flash.
        /// </summary>
        public float FinalAlpha = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Flash.
        /// </summary>
        /// <param name="color">The Color of the Flash.</param>
        public Flash(Color color) : base(0, 0)
        {
            Color = color;
        }

        #endregion

        #region Public Methods

        public override void Added()
        {
            base.Added();

            if (LifeSpan == 0)
            {
                LifeSpan = DefaultLifeSpan;
            }

            shape = Shape.CreateRectangle(Game.Instance.Width, Game.Instance.Height, Color);
            shape.Scroll = 0;
            shape.Scale = 1 / Game.Instance.CameraManager.ActiveCamera.Scale;

            Graphic = shape;
        }

        public override void Update()
        {
            base.Update();

            shape.Scale = 1 / Game.Instance.CameraManager.ActiveCamera.Scale;

            shape.Alpha = Util.ScaleClamp(Timer, 0, LifeSpan, Alpha, FinalAlpha);
        }

        public override void Removed()
        {
            base.Removed();

            ClearGraphics();
        }

        #endregion
    }
}
