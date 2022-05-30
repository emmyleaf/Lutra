using System.Numerics;
using Lutra.Rendering;
using Lutra.Rendering.Text;

namespace Lutra.Graphics;

/// <summary>
/// Graphic used to display simple text. Much faster than RichText, but more limited options.
/// </summary>
public class Text : SpriteGraphic
{
    public readonly Font Font;
    public string String;
    public int Size;
    public bool Bold;

    public Text(string str, Font font, int size, bool bold = false)
    {
        Font = font;
        String = str;
        Size = size;
        Bold = bold;

        // Font.PreloadASCII(Size, Bold);
        Texture = Font.GetTexture(Size, Bold);
        UpdateDrawable();
    }

    // TODO: This is temporary and should be on properties I guess?
    public void SetString(string str)
    {
        String = str;
        UpdateDrawable();
    }

    protected override void UpdateDrawable()
    {
        InitializeDrawable(Texture, WorldMatrix);

        Width = 0;
        Height = 0;

        var penPosition = Vector2.Zero;
        for (int i = 0; i < String.Length; i++)
        {
            var c = String[i];

            if (c == ' ')
            {
                penPosition.X += Font.GetAdvanceSpace(Size, Bold);
                continue;
            }
            if (c == '\n')
            {
                penPosition.X = 0;
                penPosition.Y += Font.GetLineSpacing(Size, Bold);
                continue;
            }

            var glyph = Font.GetGlyph(c, Size, Bold);

            // Draw
            var glyphPosition = new Vector2(penPosition.X + glyph.BearingX, penPosition.Y - glyph.BearingY);
            var sourceRect = Font.GetGlyphSourceRect(c, Size, Bold);

            SpriteDrawable.AddInstance(glyphPosition, sourceRect, Color);

            // After draw
            penPosition.X += glyph.Advance;

            if (penPosition.X > Width) Width = (int)MathF.Ceiling(penPosition.X);

            if (i < String.Length - 1)
            {
                var nextC = String[i + 1];
                penPosition.X += Font.GetKerning(c, nextC, Size, Bold);
            }
        }

        Height = (int)MathF.Ceiling(penPosition.Y += Font.GetLineSpacing(Size, Bold));
    }
}
