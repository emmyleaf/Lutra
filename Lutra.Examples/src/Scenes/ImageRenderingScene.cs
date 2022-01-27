using Lutra.Graphics;

namespace Lutra.Examples
{
    public class ImageRenderingScene : Scene
    {
        private Image theImage = null;
        private int dir = 0;

        public override void Begin()
        {
            theImage = new Image("lutra.png");
            theImage.CenterOrigin();
            theImage.X = Game.Width / 2;
            theImage.Y = Game.Height / 2;
            AddGraphic(theImage);
        }

        public override void Update()
        {
            if (dir == 0)
            {
                theImage.Angle += 1f;
                theImage.ScaleX *= 1.001f;
                theImage.ScaleY *= 1.001f;
                if (theImage.Angle >= 360f)
                {
                    dir = 1;
                }
            }
            else
            {
                theImage.Angle -= 1f;
                theImage.ScaleX *= 0.999f;
                theImage.ScaleY *= 0.999f;
                if (theImage.Angle <= 0.0f)
                {
                    dir = 0;
                }
            }

            theImage.Color = theImage.Color.WithGreen(1 - (theImage.Angle / 360f));
        }
    }
}
