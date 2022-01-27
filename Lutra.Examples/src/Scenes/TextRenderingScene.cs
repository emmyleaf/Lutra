using System.Numerics;
using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Examples
{
    public class TextRenderingScene : Scene
    {
        public override void Begin()
        {
            var testFont = AssetManager.GetFont("monogram-extended.ttf", false);
            var testEntity = new Entity(20, 100);

            var testText = new Text("Waltz, bad nymph, for quick jigs vex.", testFont, 32, false);
            var testTextBold = new Text("Sphinx of black quartz, judge my vow.", testFont, 32, true);
            testTextBold.Position = new Vector2(20, 100);

            testEntity.AddGraphic(testText);
            testEntity.AddGraphic(testTextBold);
            Add(testEntity);
        }
    }
}
