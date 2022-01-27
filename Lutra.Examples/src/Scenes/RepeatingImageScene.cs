using Lutra.Graphics;

namespace Lutra.Examples
{
    public class RepeatingImageScene : Scene
    {
        private Image repeating;
        // private int dir = 0;

        public override void Begin()
        {
            repeating = new Image("TransparentTester.png")
            {
                X = Game.Width / 2,
                Y = Game.Height / 2,
                RepeatX = true,
                RepeatY = true,
                RepeatFlipX = true,
                RepeatFlipY = true
            };
            repeating.CenterOrigin();
            AddGraphic(repeating);
        }

        public override void Update()
        {
            // if (dir == 0)
            // {
            //     repeating.Rotation += 1f;
            //     repeating.Scale *= 1.001f;
            //     if (repeating.Rotation >= 360f)
            //     {
            //         dir = 1;
            //     }
            // }
            // else
            // {
            //     repeating.Rotation -= 1f;
            //     repeating.Scale *= 0.999f;
            //     if (repeating.Rotation <= 0.0f)
            //     {
            //         dir = 0;
            //     }
            // }

            // repeating.Color = repeating.Color.WithGreen(1 - (repeating.Rotation / 360f));
        }
    }
}
