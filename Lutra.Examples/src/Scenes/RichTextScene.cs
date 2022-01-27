using ImGuiNET;
using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Examples
{
    public class RichTextScene : Scene
    {
        RichText richText;
        string textContent = "Waltz, bad nymph, for quick jigs vex.";

        public override void Begin()
        {
            richText = new RichText(textContent, AssetManager.GetFont("monogram-extended.ttf", false), 32);

            var testEntity = new Entity(20, 100) { Graphic = richText };
            Add(testEntity);

            IMGUIDraw += RichTextMenu;
        }

        private void RichTextMenu()
        {
            ImGui.Begin("Rich Text Widgetifier");

            if (ImGui.Button("Plain"))
            {
                richText.String = textContent;
            }

            if (ImGui.Button("Wavy"))
            {
                richText.String = "{waveAmpY:8}{waveRateY:2}" + textContent;
            }

            if (ImGui.Button("Shaky"))
            {
                richText.String = "{shake:2}" + textContent;
            }

            if (ImGui.Button("Colorz"))
            {
                richText.String = "{color0:a05}{color1:00bb00}{color2:889900ff}{color3:0cdf}" + textContent;
            }

            if (ImGui.Button("Shadow"))
            {
                richText.String = "{shadow:1}" + textContent;
            }

            if (ImGui.Button("Outline"))
            {
                richText.String = "{outline:1}{colorOutline:000000}" + textContent;
            }

            if (ImGui.Button("Mixture"))
            {
                richText.String = "{shadow:2}Sphinx{shadow:0} of {waveAmpY:8}{waveRateY:2}{color:000000}black {color:ffffff}quartz{waveAmpY:0}{waveRateY:0}, {shake:2}judge{shake:0} my {outline:2}{colorOutline:000000}vow.";
            }

            if (ImGui.Button("Multi Line"))
            {
                richText.String = $"{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}\n{textContent}";
            }
        }
    }
}
