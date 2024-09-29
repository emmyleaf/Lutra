using ImGuiNET;
using Lutra.Graphics;
using Lutra.Input;
using Lutra.Utility.Debugging;

namespace Lutra.Examples
{
    public class InputTestScene : Scene
    {
        private Entity theEntity;
        private Image theImage;
        private TestVirtualController Controller;

        public override void Begin()
        {
            theEntity = new Entity();
            theEntity.X = Game.Width / 2;
            theEntity.Y = Game.Height / 2;

            theImage = new Image("lutra.png");
            theImage.CenterOrigin();
            theEntity.AddGraphic(theImage);

            Controller = new TestVirtualController();
            InputManager.AddVirtualController(Controller);

            Add(theEntity);

            IMGUIDraw += ControllerMenu;
        }

        public override void Update()
        {
            theEntity.X += Controller.Movement.X * 5f;
            theEntity.Y += Controller.Movement.Y * 5f;
        }

        public class TestVirtualController : VirtualController
        {
            public VirtualAxis Movement;

            public TestVirtualController()
            {
                Movement = VirtualAxis.CreateArrowKeys();

                foreach (var controller in InputManager.Controllers)
                {
                    Movement.AddControllerAxis(controller, ControllerAxis.LeftX, ControllerAxis.LeftY);
                }

                AddAxis("Movement", Movement);
            }
        }

        private void ControllerMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Visualise (Xbox)...", InputManager.ControllersConnected > 0))
                {
                    foreach (var controller in InputManager.Controllers)
                    {
                        if (ImGui.MenuItem($"Controller {controller.ID}"))
                        {
                            ControllerDisplay.OpenControllerAsXboxIMGUIWindow(controller);
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.End();
            }

            ControllerDisplay.DrawControllerAsXboxIMGUIWindow();
        }
    }
}
