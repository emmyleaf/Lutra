using ImGuiNET;
using Lutra.Input;

namespace Lutra.Utility.Debugging
{
    public static class ControllerDisplay
    {
        private const float ControllerCenterX = 140;
        private const float ControllerCenterY = 250;

        private const float StickSize = 32;
        private const float StickIndicatorSize = 8;
        private const float LeftStickOffsetX = -80;
        private const float LeftStickOffsetY = -50;
        private const float RightStickOffsetX = 60;
        private const float RightStickOffsetY = 60;

        private const float DPadCenterOffsetX = -60;
        private const float DPadCenterOffsetY = 60;
        private const float DPadButtonWidth = 16;
        private const float DPadButtonHeight = 32;
        private const float DPadButtonDistance = 16;

        private const float FaceButtonsCenterOffsetX = 80;
        private const float FaceButtonsCenterOffsetY = -50;
        private const float FaceButtonRadius = 16;
        private const float FaceButtonDistance = 24;

        private const float LeftTriggerBumperOffsetX = -100;
        private const float LeftTriggerBumperOffsetY = -100;
        private const float RightTriggerBumperOffsetX = 100;
        private const float RightTriggerBumperOffsetY = -100;
        private const float TriggerBumperSpacing = 24;
        private const float TriggerBumperWidth = 32;
        private const float TriggerBumperHeight = 16;

        private const float StartBackSpacing = 24;
        private const float StartBackWidth = 32;
        private const float StartBackHeight = 16;

        private static readonly Color StickBorder = Color.Grey;
        private static readonly Color StickColor = Color.Pink;
        private static readonly Color StickClickColor = Color.Yellow;

        private static readonly Color DPadOffColor = Color.Grey;
        private static readonly Color DPadOnColor = Color.Pink;

        private static readonly Color TriggerOffColor = Color.Grey;
        private static readonly Color TriggerOnColor = Color.Pink;

        private static readonly Color StartBackOffColor = Color.Grey;
        private static readonly Color StartBackOnColor = Color.Pink;

        private static Controller Controller;
        private static bool WindowOpen = false;

        public static void OpenControllerAsXboxIMGUIWindow(Controller controller)
        {
            Controller = controller;
            WindowOpen = true;
        }

        public static void DrawControllerAsXboxIMGUIWindow()
        {
            if (!WindowOpen)
            {
                return;
            }

            if (ImGui.Begin($"Controller Display", ref WindowOpen))
            {
                if (Controller == null)
                {
                    ImGui.Text($"Controller Disconnected!");
                    ImGui.End();
                    return;
                }

                ImGui.Text($"Controller {Controller} Info:");
                ImGui.Text($"Name: {InputManager.GetControllerName(Controller)}");
                ImGui.Text($"Vendor ID: {InputManager.GetControllerVendorId(Controller)}");
                ImGui.Text($"Product ID: {InputManager.GetControllerProductId(Controller)}");

                ImGui.Text($"Axes & Button Display:");

                // Left Stick
                ImGuiHelper.DrawCircle(ControllerCenterX + LeftStickOffsetX, ControllerCenterY + LeftStickOffsetY, StickSize, StickBorder, true);
                ImGuiHelper.DrawCircle(
                    ControllerCenterX + LeftStickOffsetX + ((StickSize - StickIndicatorSize) * Controller.GetAxis(ControllerAxis.LeftX)),
                    ControllerCenterY + LeftStickOffsetY + ((StickSize - StickIndicatorSize) * Controller.GetAxis(ControllerAxis.LeftY)),
                    StickIndicatorSize, Controller.ButtonDown(ControllerButton.LeftStick) ? StickClickColor : StickColor, true);

                // Right Stick
                ImGuiHelper.DrawCircle(ControllerCenterX + RightStickOffsetX, ControllerCenterY + RightStickOffsetY, StickSize, StickBorder, true);
                ImGuiHelper.DrawCircle(
                    ControllerCenterX + RightStickOffsetX + ((StickSize - StickIndicatorSize) * Controller.GetAxis(ControllerAxis.RightX)),
                    ControllerCenterY + RightStickOffsetY + ((StickSize - StickIndicatorSize) * Controller.GetAxis(ControllerAxis.RightY)),
                    StickIndicatorSize, Controller.ButtonDown(ControllerButton.RightStick) ? StickClickColor : StickColor, true);

                // D-Pad (U, D, L, R)
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + DPadCenterOffsetX, ControllerCenterY + DPadCenterOffsetY - DPadButtonDistance, DPadButtonWidth, DPadButtonHeight, Controller.ButtonDown(ControllerButton.DPadUp) ? DPadOnColor : DPadOffColor, true);
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + DPadCenterOffsetX, ControllerCenterY + DPadCenterOffsetY + DPadButtonDistance, DPadButtonWidth, DPadButtonHeight, Controller.ButtonDown(ControllerButton.DPadDown) ? DPadOnColor : DPadOffColor, true);
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + DPadCenterOffsetX - DPadButtonDistance, ControllerCenterY + DPadCenterOffsetY, DPadButtonHeight, DPadButtonWidth, Controller.ButtonDown(ControllerButton.DPadLeft) ? DPadOnColor : DPadOffColor, true);
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + DPadCenterOffsetX + DPadButtonDistance, ControllerCenterY + DPadCenterOffsetY, DPadButtonHeight, DPadButtonWidth, Controller.ButtonDown(ControllerButton.DPadRight) ? DPadOnColor : DPadOffColor, true);

                // Face Buttons (A, B, X, Y)
                ImGuiHelper.DrawCircle(ControllerCenterX + FaceButtonsCenterOffsetX, ControllerCenterY + FaceButtonsCenterOffsetY + FaceButtonDistance, FaceButtonRadius, Controller.ButtonDown(ControllerButton.A) ? Color.Green : Color.Green * Color.Grey, true);
                ImGuiHelper.DrawCircle(ControllerCenterX + FaceButtonsCenterOffsetX + FaceButtonDistance, ControllerCenterY + FaceButtonsCenterOffsetY, FaceButtonRadius, Controller.ButtonDown(ControllerButton.B) ? Color.Red : Color.Red * Color.Grey, true);
                ImGuiHelper.DrawCircle(ControllerCenterX + FaceButtonsCenterOffsetX - FaceButtonDistance, ControllerCenterY + FaceButtonsCenterOffsetY, FaceButtonRadius, Controller.ButtonDown(ControllerButton.X) ? Color.Blue : Color.Blue * Color.Grey, true);
                ImGuiHelper.DrawCircle(ControllerCenterX + FaceButtonsCenterOffsetX, ControllerCenterY + FaceButtonsCenterOffsetY - FaceButtonDistance, FaceButtonRadius, Controller.ButtonDown(ControllerButton.Y) ? Color.Yellow : Color.Yellow * Color.Grey, true);

                // LT, RT
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + LeftTriggerBumperOffsetX, ControllerCenterY + LeftTriggerBumperOffsetY - TriggerBumperSpacing, TriggerBumperWidth, TriggerBumperHeight, Util.LerpColor(TriggerOffColor, TriggerOnColor, Controller.GetAxis(ControllerAxis.LeftTrigger)), true);
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + RightTriggerBumperOffsetX, ControllerCenterY + RightTriggerBumperOffsetY - TriggerBumperSpacing, TriggerBumperWidth, TriggerBumperHeight, Util.LerpColor(TriggerOffColor, TriggerOnColor, Controller.GetAxis(ControllerAxis.RightTrigger)), true);

                // LB, RB
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + LeftTriggerBumperOffsetX, ControllerCenterY + LeftTriggerBumperOffsetY, TriggerBumperWidth, TriggerBumperHeight, Controller.ButtonDown(ControllerButton.LeftShoulder) ? TriggerOnColor : TriggerOffColor, true);
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + RightTriggerBumperOffsetX, ControllerCenterY + RightTriggerBumperOffsetY, TriggerBumperWidth, TriggerBumperHeight, Controller.ButtonDown(ControllerButton.RightShoulder) ? TriggerOnColor : TriggerOffColor, true);

                // Start & Back
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX + StartBackSpacing, ControllerCenterY, StartBackWidth, StartBackHeight, Controller.ButtonDown(ControllerButton.Start) ? StartBackOnColor : StartBackOffColor, true);
                ImGuiHelper.DrawRectangleCentered(ControllerCenterX - StartBackSpacing, ControllerCenterY, StartBackWidth, StartBackHeight, Controller.ButtonDown(ControllerButton.Back) ? StartBackOnColor : StartBackOffColor, true);

                ImGui.End();
            }
        }
    }
}
