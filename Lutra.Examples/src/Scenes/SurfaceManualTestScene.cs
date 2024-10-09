using System;
using System.Collections.Generic;
using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Examples
{
    // This scene shows off manual drawing to a Surface using a custom Render method.
    public class SurfaceManualTestScene : Scene
    {
        public List<Image> Lutras = [];
        public Surface LutrasSurface;

        public override void Begin()
        {
            Game.Color = Color.Black;

            LutrasSurface = new Surface(Game.Width, Game.Height, Color.None);
            LutrasSurface.UseSceneCamera = false;
            LutrasSurface.Smooth = true;

            for (int i = 1; i <= 3; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    var lutra = new Image("lutra.png");
                    lutra.CenterOrigin();
                    lutra.X = i * Game.Width / 4;
                    lutra.Y = j * Game.Height / 4;
                    lutra.Color = Color.Red;
                    Lutras.Add(lutra);
                }
            }
        }

        public override void End()
        {
            Game.Color = Color.CornflowerBlue;
        }

        public override void Update()
        {
            foreach (var lutra in Lutras)
            {
                var lerpAmount = 0.5f - MathF.Cos(Timer * 0.6f + lutra.X + lutra.Y);
                lutra.Color = Util.LerpColor(lerpAmount, Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Magenta, Color.Red);
            }

            MainCamera.Angle = MathF.Sin(Timer * 1.2f) * 5.0f;
            MainCamera.Scale = 0.94f + (MathF.Sin(Timer * 1.8f) * 0.05f);
        }

        public override void Render()
        {
            foreach (var lutra in Lutras)
            {
                LutrasSurface.Draw(lutra);
            }

            Game.Surface.Draw(LutrasSurface);
        }
    }
}
