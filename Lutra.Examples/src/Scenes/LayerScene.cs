using ImGuiNET;
using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Examples
{
    public class LayerScene : Scene
    {
        Image image;
        int imageLayer;
        Text text;
        int textLayer;
        RichText richText1;
        int richText1Layer;
        RichText richText2;
        int richText2Layer;

        public override void Begin()
        {
            var testFont = AssetManager.GetFont("monogram-extended.ttf", false);

            image = new Image("lutra.png");
            image.CenterOrigin();
            image.X = Game.Width / 2;
            image.Y = Game.Height / 2;
            image.Layer = 0;

            text = new Text("wow that's so poggers", testFont, 48);
            text.CenterOrigin();
            text.X = Game.Width / 2;
            text.Y = Game.Height / 2 - 30;
            text.Layer = 0;

            richText1 = new RichText("what happens now???", testFont, 48);
            richText1.CenterOrigin();
            richText1.X = Game.Width / 2;
            richText1.Y = Game.Height / 2;
            richText1.Layer = 0;

            richText2 = new RichText("testing testing testing", testFont, 48);
            richText2.Color = Color.Black;
            richText2.CenterOrigin();
            richText2.X = Game.Width / 2;
            richText2.Y = Game.Height / 2 + 30;
            richText2.Layer = 0;

            AddGraphic(text);
            AddGraphic(image);
            AddGraphic(richText1);
            AddGraphic(richText2);

            IMGUIDraw += DrawImGui;
        }

        public override void Update()
        {
            image.Layer = imageLayer;
            text.Layer = textLayer;
            richText1.Layer = richText1Layer;
            richText2.Layer = richText2Layer;
        }

        private void DrawImGui()
        {
            ImGui.Begin("Layers");
            ImGui.DragInt("Image Layer", ref imageLayer);
            ImGui.DragInt("Text Layer", ref textLayer);
            ImGui.DragInt("Rich Text 1 Layer", ref richText1Layer);
            ImGui.DragInt("Rich Text 2 Layer", ref richText2Layer);
            ImGui.End();
        }
    }
}
