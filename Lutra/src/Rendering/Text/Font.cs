using System.Collections.Generic;
using Lutra.Utility;
using SharpFont;

namespace Lutra.Rendering.Text
{
    public class Font
    {
        private static Library Library;

        private readonly Face Face;
        private readonly LoadTarget loadTarget;
        private readonly Dictionary<int, FontPage> FontPages = new();
        private int CurrentSize;
        private List<int> preloadedKeys = new();

        static Font()
        {
            FreetypeDllResolver.Register();
            Library = new Library();
        }

        public Font(byte[] fontBytes, bool antialiased = true)
        {
            Face = new Face(Library, fontBytes, 0);
            loadTarget = antialiased ? LoadTarget.Normal : LoadTarget.Mono;
        }

        public void PreloadASCII(int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);
            if (preloadedKeys.Contains(key)) return;

            for (int c1 = 32; c1 <= 126; c1++)
            {
                GetGlyph((char)c1, size, bold);
                for (int c2 = 32; c2 <= 126; c2++)
                {
                    GetKerning((char)c1, (char)c2, size, bold);
                }
            }

            preloadedKeys.Add(key);
        }

        public Glyph GetGlyph(char c, int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);

            if (!FontPages.TryGetValue(key, out var fontPage))
            {
                FontPages[key] = fontPage = CreateFontPage(size, bold);
            }

            if (!fontPage.Glyphs.TryGetValue(c, out var glyph))
            {
                glyph = LoadGlyph(fontPage, c);
            }

            return glyph;
        }

        public RectInt GetGlyphSourceRect(char c, int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);

            if (!FontPages.TryGetValue(key, out var fontPage))
            {
                return RectInt.Empty;
            }

            if (!fontPage.GlyphSourceRects.TryGetValue(c, out var rect))
            {
                return RectInt.Empty;
            }

            return rect;
        }

        public float GetLineSpacing(int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);

            if (!FontPages.TryGetValue(key, out var fontPage))
            {
                FontPages[key] = fontPage = CreateFontPage(size, bold);
            }

            return fontPage.LineHeight;
        }

        public float GetAdvanceSpace(int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);

            if (!FontPages.TryGetValue(key, out var fontPage))
            {
                FontPages[key] = fontPage = CreateFontPage(size, bold);
            }

            return fontPage.AdvanceSpace;
        }

        public LutraTexture GetTexture(int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);

            if (!FontPages.TryGetValue(key, out var fontPage))
            {
                FontPages[key] = fontPage = CreateFontPage(size, bold);
            }

            return fontPage.Texture;
        }

        public float GetKerning(char first, char second, int size, bool bold = false)
        {
            var key = FontPageKey(size, bold);

            if (!Face.HasKerning || !FontPages.TryGetValue(key, out var fontPage))
            {
                return 0;
            }

            if (!fontPage.Kerning.TryGetValue(first, out var charKerning))
            {
                fontPage.Kerning[first] = charKerning = new();
            }

            if (!charKerning.TryGetValue(second, out var kerning))
            {
                charKerning[second] = kerning = (int)Face.GetKerning(Face.GetCharIndex(first), Face.GetCharIndex(second), KerningMode.Default).X;
            }

            return kerning;
        }

        #region Implementation

        private static int FontPageKey(int size, bool bold)
        {
            return bold ? -size : size;
        }

        private void SetCurrentSize(int size)
        {
            if (CurrentSize != size)
            {
                CurrentSize = size;
                Face.SetPixelSizes(0, (uint)size);
            }
        }

        private FontPage CreateFontPage(int size, bool bold)
        {
            SetCurrentSize(size);

            var lineHeight = Face.Size.Metrics.NominalHeight;

            Face.LoadGlyph(Face.GetCharIndex(32), LoadFlags.ForceAutohint, loadTarget);
            var advanceSpace = (int)Face.Glyph.Metrics.HorizontalAdvance;

            return new FontPage(size, bold, lineHeight, advanceSpace);
        }

        private Glyph LoadGlyph(FontPage fontPage, char c)
        {
            SetCurrentSize(fontPage.Size);

            var glyphIndex = Face.GetCharIndex(c);
            Face.LoadGlyph(glyphIndex, LoadFlags.ForceAutohint, loadTarget);

            if (fontPage.Bold)
            {
                Face.Glyph.Outline.Embolden(1);
            }

            byte[] bufferData;

            if (Face.Glyph.Metrics.Width == 0)
            {
                bufferData = new byte[0];
            }
            else
            {
                Face.Glyph.RenderGlyph(RenderMode.Normal);
                bufferData = Face.Glyph.Bitmap.BufferData;
            }

            var glyph = new Glyph(c, glyphIndex, Face.Glyph.Metrics, bufferData);

            if (fontPage.Bold)
            {
                glyph.Advance += 1;
                glyph.Width = Face.Glyph.Bitmap.Width;
                glyph.Height = Face.Glyph.Bitmap.Rows;
            }

            fontPage.Glyphs[c] = glyph;

            var rect = fontPage.Texture.FindRect(glyph.Width, glyph.Height);
            fontPage.GlyphSourceRects[c] = rect;

            fontPage.Texture.RenderGlyph(glyph.Width, glyph.Height, bufferData, rect.X, rect.Y);

            return glyph;
        }

        #endregion
    }
}
