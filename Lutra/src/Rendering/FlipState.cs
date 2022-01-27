namespace Lutra.Rendering;

[Flags]
public enum FlipState
{
    Normal = 0,
    FlippedX = 1,
    FlippedY = 2,
    FlippedXY = FlippedX | FlippedY
}
