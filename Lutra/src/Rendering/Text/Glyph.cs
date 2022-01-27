using SharpFont;

namespace Lutra.Rendering.Text
{
    public struct Glyph
    {
        public uint Index;
        public int Width;
        public int Height;
        public int Advance;
        public int BearingX;
        public int BearingY;
        public char Character;
        public byte[] BufferData;

        public Glyph(char c, uint index, GlyphMetrics metrics, byte[] bufferData)
        {
            Index = index;
            Character = c;
            // Shift 1/64px values into integers
            Width = metrics.Width.Value >> 6;
            Height = metrics.Height.Value >> 6;
            Advance = metrics.HorizontalAdvance.Value >> 6;
            BearingX = metrics.HorizontalBearingX.Value >> 6;
            BearingY = metrics.HorizontalBearingY.Value >> 6;
            BufferData = bufferData;
        }
    }
}
