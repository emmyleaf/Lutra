using System;
using System.Collections.Generic;
using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Examples
{
    // This scene shows off managed drawing to a Surface using Layer property to set render order.
    public class SurfaceLayeredTestScene : Scene
    {
        public List<Image> Lutras = [];
        public Surface LutrasSurface;

        public override void Begin()
        {
            Game.Color = Color.Black;

            LutrasSurface = new Surface(Game.Width, Game.Height, Color.None);
            LutrasSurface.UseSceneCamera = false;
            LutrasSurface.Smooth = true;
            LutrasSurface.Layer = -10;
            AddGraphic(LutrasSurface);

            for (int i = 1; i <= 3; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    var lutra = new Image("lutra.png");
                    lutra.CenterOrigin();
                    lutra.X = i * Game.Width / 4;
                    lutra.Y = j * Game.Height / 4;
                    lutra.Layer = 10;
                    lutra.Color = Color.Red;
                    lutra.AddSurface(LutrasSurface);
                    AddGraphic(lutra);
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
    }
}
