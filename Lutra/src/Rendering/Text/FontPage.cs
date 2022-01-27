using System.Collections.Generic;
using Lutra.Utility;

namespace Lutra.Rendering.Text
{
    internal class FontPage
    {
        internal FontPageTexture Texture { get; private set; }
        internal int Size { get; private set; }
        internal bool Bold { get; private set; }
        internal int LineHeight { get; private set; }
        internal int AdvanceSpace { get; private set; }

        internal readonly Dictionary<char, Glyph> Glyphs = new();
        internal readonly Dictionary<char, RectInt> GlyphSourceRects = new Dictionary<char, RectInt>();
        internal readonly Dictionary<char, Dictionary<char, int>> Kerning = new();

        internal FontPage(int size, bool bold, int lineHeight, int advanceSpace)
        {
            Texture = new FontPageTexture();
            Size = size;
            Bold = bold;
            LineHeight = lineHeight;
            AdvanceSpace = advanceSpace;
        }
    }
}
