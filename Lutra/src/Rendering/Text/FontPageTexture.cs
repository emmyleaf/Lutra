using System.Collections.Generic;
using Lutra.Utility;

namespace Lutra.Rendering.Text
{
    internal class FontPageTexture : LutraTexture
    {
        public static ushort NewTextureSize = 512;
        private readonly List<RectInt> nodes = new List<RectInt>();

        public FontPageTexture() : base(NewTextureSize)
        {
            nodes.Add(new RectInt(0, 0, NewTextureSize, NewTextureSize));
        }

        public RectInt FindRect(int w, int h)
        {
            // Pad around glyphs to prevent bleeding
            w += 2;
            h += 2;

            var rect = RectInt.Empty;

            if (!TryFindRect(w, h, ref rect))
            {
                EnlargeTexture();
                TryFindRect(w, h, ref rect);
            };

            return rect;
        }

        private bool TryFindRect(int w, int h, ref RectInt rect)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];

                if (w <= node.Width && h <= node.Height)
                {
                    nodes.RemoveAt(i);

                    rect = new RectInt(node.X, node.Y, w, h);
                    nodes.Add(new RectInt(rect.Right, rect.Y, node.Right - rect.Right, rect.Height));
                    nodes.Add(new RectInt(rect.X, rect.Bottom, rect.Width, node.Bottom - rect.Bottom));
                    nodes.Add(new RectInt(rect.Right, rect.Bottom, node.Right - rect.Right, node.Bottom - rect.Bottom));

                    // Offset for padding
                    rect.X += 1;
                    rect.Y += 1;
                    rect.Width -= 2;
                    rect.Height -= 2;

                    return true;
                }
            }

            return false;
        }

        private void EnlargeTexture()
        {
            // var oldRect = Texture.Bounds;
            // var oldData = new Color[oldRect.Width * oldRect.Height];
            // Texture.GetData(oldData);

            // Texture = new Texture2D(Game.Instance.GraphicsDevice, oldRect.Width * 2, oldRect.Height * 2, false, SurfaceFormat.Color);
            // Texture.SetData(0, oldRect, oldData, 0, oldData.Length);

            // nodes.Add(new Rect(0, oldRect.Bottom, oldRect.Width, oldRect.Height));
            // nodes.Add(new Rect(oldRect.Right, 0, oldRect.Width, oldRect.Height * 2));
        }

        public void RenderGlyph(int glyphWidth, int glyphHeight, byte[] bitmap, int x, int y)
        {
            var colors = new byte[glyphWidth * glyphHeight * 4];

            int i = 0;
            foreach (byte alpha in bitmap)
            {
                colors[i] = 255;
                colors[i + 1] = 255;
                colors[i + 2] = 255;
                colors[i + 3] = alpha;
                i += 4;
            }

            VeldridResources.UpdateTexture(Texture, colors, new RectInt(x, y, glyphWidth, glyphHeight));
        }
    }
}
