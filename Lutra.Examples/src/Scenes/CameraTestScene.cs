using ImGuiNET;
using Lutra.Cameras;
using Lutra.Graphics;

namespace Lutra.Examples
{
    public class CameraTestScene : Scene
    {
        private Image theImage = null;
        private Camera theCamera = null;
        private float camX, camY, camWidth, camHeight, camAngle, camScale;

        public override void Begin()
        {
            theCamera = new Camera(Game);
            Game.CameraManager.PushCamera(theCamera);

            PullCamVars();

            theImage = new Image("lutra.png");
            theImage.CenterOrigin();
            theImage.X = Game.Width / 2;
            theImage.Y = Game.Height / 2;
            AddGraphic(theImage);

            IMGUIDraw += CameraControlIMGUI;
        }

        public void CameraControlIMGUI()
        {
            if (ImGui.Begin("Camera Controls"))
            {
                if (ImGui.DragFloat("Camera X", ref camX)) PushCamVars();
                if (ImGui.DragFloat("Camera Y", ref camY)) PushCamVars();
                if (ImGui.DragFloat("Camera Width", ref camWidth)) PushCamVars();
                if (ImGui.DragFloat("Camera Height", ref camHeight)) PushCamVars();
                if (ImGui.DragFloat("Camera Angle", ref camAngle)) PushCamVars();
                if (ImGui.DragFloat("Camera Scale", ref camScale, 0.01f, 0.01f, 10.0f)) PushCamVars();

                ImGui.Text($"Controlling camera: {Game.CameraManager.CameraCount - 1}");

                if (ImGui.Button("Push Camera"))
                {
                    theCamera = new Camera(Game);
                    Game.CameraManager.PushCamera(theCamera);
                    PullCamVars();
                }

                if (ImGui.Button("Pop Camera"))
                {
                    Game.CameraManager.PopCamera();
                    theCamera = Game.CameraManager.ActiveCamera;
                    PullCamVars();
                }

                if (ImGui.Button("Clear All"))
                {
                    Game.CameraManager.ClearStack();
                    theCamera = Game.CameraManager.ActiveCamera;
                    PullCamVars();
                }

                ImGui.End();
            }
        }

        private void PullCamVars()
        {
            camX = theCamera.X;
            camY = theCamera.Y;
            camWidth = theCamera.Width;
            camHeight = theCamera.Height;
            camAngle = theCamera.Angle;
            camScale = theCamera.Scale;
        }

        private void PushCamVars()
        {
            theCamera.X = camX;
            theCamera.Y = camY;
            theCamera.Width = camWidth;
            theCamera.Height = camHeight;
            theCamera.Angle = camAngle;
            theCamera.Scale = camScale;
        }
    }
}

