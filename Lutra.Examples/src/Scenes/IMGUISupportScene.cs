using ImGuiNET;
using Lutra.Graphics;
using Lutra.Rendering.Text;
using Lutra.Utility;
using Lutra.Utility.Debugging;

namespace Lutra.Examples
{
    public class IMGUISupportScene : Scene
    {
        Font font;
        string textContent = "yosawup";
        Text theText = null;
        bool textBold = false;
        bool editingText = false;

        public override void Begin()
        {
            font = AssetManager.GetFont("monogram-extended.ttf", false);

            IMGUIDraw += DrawMyUI;

            // Pop open the console.
            DebugConsole.Instance.IsOpen = true;
        }

        private void DrawMyUI()
        {
            if (editingText)
            {
                ImGui.Begin("IMGUI Support");
                if (theText == null)
                {
                    ImGui.InputText("Text String", ref textContent, 100);
                    ImGui.Checkbox("Text Bold?", ref textBold);
                    if (ImGui.Button("spawn text"))
                    {
                        theText = new Text(textContent, font, 32, textBold);
                        theText.X = 30;
                        theText.Y = 100;

                        AddGraphic(theText);
                    }
                }
                else
                {
                    // oops i broke this
                    // ImGui.DragFloat("Text X", ref theText.X);
                    // ImGui.DragFloat("Text Y", ref theText.Y);
                    ImGui.InputText("Text String", ref theText.String, 100);
                    ImGui.InputInt("Text Size", ref theText.Size);
                    ImGui.Checkbox("Text Bold?", ref theText.Bold);
                }

                if (ImGui.Button("Close"))
                {
                    editingText = false;
                }

                ImGui.End();
            }

            if (ImGui.BeginPopup("ConfirmAllDelete"))
            {
                ImGui.Text("Are you sure?");
                if (ImGui.Button("Yes"))
                {
                    RemoveAllGraphics();
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("No"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }


            bool openedConfirmPopup = false;
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Create"))
                {
                    if (ImGui.MenuItem("New Text"))
                    {
                        theText = new Text("New Text", font, 32, textBold);
                        theText.X = 100;
                        theText.Y = 100;
                        AddGraphic(theText);
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Edit Text Object"))
                    {
                        editingText = true;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Destroy"))
                {
                    if (ImGui.MenuItem("All"))
                    {
                        openedConfirmPopup = true;
                    }
                    ImGui.EndMenu();
                }


                ImGui.EndMainMenuBar();
            }

            if (openedConfirmPopup)
            {
                ImGui.OpenPopup("ConfirmAllDelete");
            }
        }
    }
}
