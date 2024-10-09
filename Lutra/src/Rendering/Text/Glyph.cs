using SharpFont;

namespace Lutra.Rendering.Text
{
    public struct Glyph(char c, uint index, GlyphMetrics metrics, byte[] bufferData)
    {
        public uint Index = index;
        public int Width = metrics.Width.Value >> 6;
        public int Height = metrics.Height.Value >> 6;
        public int Advance = metrics.HorizontalAdvance.Value >> 6;
        public int BearingX = metrics.HorizontalBearingX.Value >> 6;
        public int BearingY = metrics.HorizontalBearingY.Value >> 6;
        public char Character = c;
        public byte[] BufferData = bufferData;
    }
}
