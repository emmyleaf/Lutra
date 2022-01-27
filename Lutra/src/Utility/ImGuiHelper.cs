using System.Numerics;
using ImGuiNET;

namespace Lutra.Utility
{
    public static class ImGuiHelper
    {
        public static ImColor ImColorFromColor(Color color)
        {
            return new ImColor { Value = color.ToVector4() };
        }

        public static uint ConvertColorToU32(Color color)
        {
            var src = new[] { color.ByteR, color.ByteG, color.ByteB, color.ByteA };
            return BitConverter.ToUInt32(src);
        }

        public static void DrawRectangle(float x, float y, float w, float h, Color color, bool local = false)
        {
            var drawListPtr = local ? ImGui.GetWindowDrawList() : ImGui.GetForegroundDrawList();
            var offset = local ? ImGui.GetWindowPos() : Vector2.Zero;

            drawListPtr.AddRect(new Vector2(x, y) + offset, new Vector2(x + w, y + h) + offset, ConvertColorToU32(color));
        }

        public static void DrawRectangleCentered(float x, float y, float w, float h, Color color, bool local = false)
        {
            var drawListPtr = local ? ImGui.GetWindowDrawList() : ImGui.GetForegroundDrawList();
            var offset = local ? ImGui.GetWindowPos() : Vector2.Zero;

            drawListPtr.AddRect(new Vector2(x - w / 2, y - h / 2) + offset, new Vector2(x + w / 2, y + h / 2) + offset, ConvertColorToU32(color));
        }

        public static void DrawCircle(float x, float y, float radius, Color color, bool local = false)
        {
            var drawListPtr = local ? ImGui.GetWindowDrawList() : ImGui.GetForegroundDrawList();
            var offset = local ? ImGui.GetWindowPos() : Vector2.Zero;

            drawListPtr.AddCircle(new Vector2(x, y) + offset, radius, ConvertColorToU32(color));
        }

        public static void DrawLine(float sx, float sy, float ex, float ey, Color color, bool local = false)
        {
            var drawListPtr = local ? ImGui.GetWindowDrawList() : ImGui.GetForegroundDrawList();
            var offset = local ? ImGui.GetWindowPos() : Vector2.Zero;

            drawListPtr.AddLine(new Vector2(sx, sy) + offset, new Vector2(ex, ey) + offset, ConvertColorToU32(color));
        }

        public static void DrawPolygon(Polygon polygon, Color color, float offsetX, float offsetY, bool local = false)
        {
            var windowOffset = local ? ImGui.GetWindowPos() : Vector2.Zero;

            for (int pointIdx = 0; pointIdx < polygon.Points.Count; pointIdx++)
            {
                var point = polygon.Points[pointIdx];
                var point2 = pointIdx != polygon.Points.Count - 1 ? polygon.Points[pointIdx + 1] : polygon.Points[0];

                DrawLine(point.X + offsetX + windowOffset.X, point.Y + offsetY + windowOffset.Y, point2.X + offsetX + windowOffset.X, point2.Y + offsetY + windowOffset.Y, color);
            }
        }
    }
}
