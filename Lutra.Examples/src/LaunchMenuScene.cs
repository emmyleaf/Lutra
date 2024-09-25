using ImGuiNET;
using Lutra.Examples.Microgames.LType;
using Lutra.Examples.Microgames.Snake;

namespace Lutra.Examples
{
    public class LaunchMenuScene : Scene
    {
        public LaunchMenuScene()
        {
            IMGUIDraw += DrawMenu;
        }

        private void DrawMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Start Microgame"))
                {
                    if (ImGui.MenuItem("L-Type Engram Alpha"))
                    {
                        Game.AddScene(new LTypeScene());
                    }

                    if (ImGui.MenuItem("Snake"))
                    {
                        Game.AddScene(new SnakeScene());
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Start Test Scene"))
                {
                    if (ImGui.MenuItem("Blank Scene"))
                    {
                        Game.AddScene(new Scene());
                    }
                    if (ImGui.MenuItem("Text Rendering"))
                    {
                        Game.AddScene(new TextRenderingScene());
                    }
                    if (ImGui.MenuItem("Image Rendering"))
                    {
                        Game.AddScene(new ImageRenderingScene());
                    }
                    if (ImGui.MenuItem("Repeating Images"))
                    {
                        Game.AddScene(new RepeatingImageScene());
                    }
                    if (ImGui.MenuItem("IMGUI Support"))
                    {
                        Game.AddScene(new IMGUISupportScene());
                    }
                    if (ImGui.MenuItem("Input Test"))
                    {
                        Game.AddScene(new InputTestScene());
                    }
                    if (ImGui.MenuItem("Pong"))
                    {
                        Game.AddScene(new PongTestScene());
                    }
                    if (ImGui.MenuItem("Collision"))
                    {
                        Game.AddScene(new ColliderTestScene());
                    }
                    if (ImGui.MenuItem("Layer Test"))
                    {
                        Game.AddScene(new LayerScene());
                    }
                    if (ImGui.MenuItem("Rich Text :)"))
                    {
                        Game.AddScene(new RichTextScene());
                    }
                    if (ImGui.MenuItem("Camera"))
                    {
                        Game.AddScene(new CameraTestScene());
                    }
                    if (ImGui.MenuItem("OpenAL Sounds"))
                    {
                        Game.AddScene(new OpenALSoundsScene());
                    }
                    if (ImGui.MenuItem("Surface Rendering (Manual)"))
                    {
                        Game.AddScene(new SurfaceManualTestScene());
                    }
                    if (ImGui.MenuItem("Surface Rendering (Layered)"))
                    {
                        Game.AddScene(new SurfaceLayeredTestScene());
                    }
                    if (ImGui.MenuItem("Asset Archives"))
                    {
                        Game.AddScene(new AssetArchiveTestScene());
                    }
                    if (ImGui.MenuItem("MIDI Controller"))
                    {
                        Game.AddScene(new MIDIScene());
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }
    }
}
