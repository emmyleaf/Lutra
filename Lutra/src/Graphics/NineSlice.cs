using System.Collections.Generic;
using Lutra.Utility;

namespace Lutra.Graphics
{
    /// <summary>
    /// Graphic type used to render a panel made up of 9 slices of an image.
    /// Handy for rendering panels with border graphics.
    /// </summary>
    public class NineSlice : SpriteGraphic
    {

        #region Static Methods

        /// <summary>
        /// Register a fill rectangle for a specific asset.  Useful for not having to set the same fill rect
        /// every time you use a NineSlice for a specific image.
        /// </summary>
        /// <param name="key">The asset path.</param>
        /// <param name="x1">Fill rect x1.</param>
        /// <param name="y1">Fill Rect y1</param>
        /// <param name="x2">Fill rect x2.</param>
        /// <param name="y2">Fill Rect y2</param>
        public static void SetFillRect(string key, int x1, int y1, int x2, int y2)
        {
            var rect = new RectInt()
            {
                X = x1,
                Y = y1,
                Width = x2 - x1,
                Height = y2 - y1
            };

            if (!fillRects.ContainsKey(key))
            {
                fillRects.Add(key, rect);
            }
            fillRects[key] = rect;
        }

        /// <summary>
        /// Sets the FillRect for a NineSlice globally.
        /// </summary>
        /// <param name="keys">The source Texture of the NineSlice. (File path or name on Atlas both work.)</param>
        /// <param name="x1">The left corner of the fill rectangle.</param>
        /// <param name="y1">The top corner of the fill rectangle.</param>
        /// <param name="x2">The right corner of the fill rectangle.</param>
        /// <param name="y2">The bottom corner of the fill rectangle.</param>
        public static void SetFillRect(string[] keys, int x1, int y1, int x2, int y2)
        {
            foreach (var k in keys)
            {
                SetFillRect(k, x1, y1, x2, y2);
            }
        }

        #endregion

        #region Private Fields

        int
            tWidth,
            tHeight;

        static Dictionary<string, RectInt> fillRects = new Dictionary<string, RectInt>();

        int sliceX1, sliceX2, sliceY1, sliceY2;

        PanelType paneltype;

        float panelScaleX, panelScaleY;

        #endregion

        #region Public Fields

        /// <summary>
        /// When using PanelType.Tiled snap the width to increments of the tile width.
        /// </summary>
        public bool SnapWidth;

        /// <summary>
        /// When using PanelType.Tiled snap the height to increments of the tile height.
        /// </summary>
        public bool SnapHeight;

        /// <summary>
        /// Determines how the size of the panel will be adjusted when setting PanelWidth and PanelHeight.
        /// If set to All, the entire panel will be the width and height.
        /// If set to Inside, the inside of the panel will be the width and height.
        /// </summary>
        public PanelSizeMode PanelSizeMode = PanelSizeMode.All;

        #endregion

        #region Public Properties

        /// <summary>
        /// The type of panel to use for the NineSlice.
        /// </summary>
        public PanelType PanelType
        {
            get
            {
                return paneltype;
            }
            set
            {
                paneltype = value;
                NeedsUpdate = true;
            }
        }

        /// <summary>
        /// Set the panel width of the NineSlice.  This will update and rerender it.
        /// </summary>
        public int PanelWidth
        {
            set
            {
                if (PanelSizeMode == PanelSizeMode.Inside)
                {
                    value += sliceX1 + tWidth - sliceX2;
                }

                if (SnapWidth)
                {
                    Width = GetSnapWidth(value);
                }
                else
                {
                    Width = value;
                }
                if (Width < 1) Width = 1;
                NeedsUpdate = true;
            }
            get
            {
                if (PanelSizeMode == PanelSizeMode.Inside)
                {
                    return Width - sliceX1 + tWidth - sliceX2;
                }

                return Width;
            }
        }

        /// <summary>
        /// Set the panel height of the NineSlice.  This will update and rerender it.
        /// </summary>
        public int PanelHeight
        {
            set
            {
                if (PanelSizeMode == PanelSizeMode.Inside)
                {
                    value += sliceY1 + tHeight - sliceY2;
                }

                if (SnapHeight)
                {
                    Height = GetSnapHeight(value);
                }
                else
                {
                    Height = value;
                }
                if (Height < 1) Height = 1;
                NeedsUpdate = true;
            }
            get
            {
                if (PanelSizeMode == PanelSizeMode.Inside)
                {
                    return Height - sliceY1 + tHeight - sliceY2;
                }

                return Height;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new NineSlice with a file path to a Texture.
        /// </summary>
        /// <param name="source">The file path to the Texture.</param>
        /// <param name="width">The width of the NineSlice panel.</param>
        /// <param name="height">The height of the NineSlice panel.</param>
        /// <param name="fillRect">The rectangle to determine the stretched areas.</param>
        public NineSlice(string source, int width = 0, int height = 0, RectInt? fillRect = null)
            : base()
        {
            SetTexture(AssetManager.GetTexture(source));
            Initialize(source, width, height, fillRect);
        }

        #endregion

        #region Private Methods

        void Initialize(string source, int width, int height, RectInt? fillRect)
        {
            tWidth = TextureRegion.Width;
            tHeight = TextureRegion.Height;

            if (width == 0 || height == 0)
            {
                Width = tWidth;
                Height = tHeight;
            }
            else
            {
                Width = width;
                Height = height;
            }

            sliceX1 = tWidth / 3;
            sliceX2 = tWidth / 3 * 2;
            sliceY1 = tHeight / 3;
            sliceY2 = tHeight / 3 * 2;

            if (fillRect == null)
            {
                if (fillRects.ContainsKey(source))
                {
                    var rect = fillRects[source];
                    SetFillRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
                }
            }
            else
            {
                var rect = fillRect.Value;
                SetFillRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }

        protected override void UpdateDrawable()
        {
            InitializeDrawable(Texture, WorldMatrix);

            var minWidth = sliceX1 + tWidth - sliceX2;
            panelScaleX = (float)Width / (float)minWidth;
            if (panelScaleX > 1) panelScaleX = 1;

            var minHeight = sliceY1 + tHeight - sliceY2;
            panelScaleY = (float)Height / (float)minHeight;
            if (panelScaleY > 1) panelScaleY = 1;

            int x0 = 0;
            int y0 = 0;
            int x1 = sliceX1;
            int y1 = sliceY1;
            int x2 = Width - (tWidth - sliceX2);
            int y2 = Height - (tHeight - sliceY2);
            int x3 = Width;
            int y3 = Height;

            int u0 = TextureRegion.Left;
            int v0 = TextureRegion.Top;
            int u1 = TextureRegion.Left + sliceX1;
            int v1 = TextureRegion.Top + sliceY1;
            int u2 = TextureRegion.Left + sliceX2;
            int v2 = TextureRegion.Top + sliceY2;
            int u3 = TextureRegion.Left + tWidth;
            int v3 = TextureRegion.Top + tHeight;

            if (panelScaleX < 1)
            {
                x1 = (int)Math.Round(x1 * panelScaleX);
                x2 = x1;
            }
            if (panelScaleY < 1)
            {
                y1 = (int)Math.Round(y1 * panelScaleY);
                y2 = y1;
            }

            int tileX = sliceX2 - sliceX1;
            int tileY = sliceY2 - sliceY1;

            if (PanelType == PanelType.Stretch)
            {
                //top
                SpriteDrawable.AddInstance(x1, y0, x2, y1, u1, v0, u2, v1, Color);

                //left
                SpriteDrawable.AddInstance(x0, y1, x1, y2, u0, v1, u1, v2, Color);

                //right
                SpriteDrawable.AddInstance(x2, y1, x3, y2, u2, v1, u3, v2, Color);

                //bottom
                SpriteDrawable.AddInstance(x1, y2, x2, y3, u1, v2, u2, v3, Color);

                //middle
                SpriteDrawable.AddInstance(x1, y1, x2, y2, u1, v1, u2, v2, Color);
            }
            else
            {
                for (int xx = x1; xx < x2; xx += tileX)
                {
                    for (int yy = y1; yy < y2; yy += tileY)
                    {
                        //middle
                        SpriteDrawable.AddInstance(xx, yy, xx + tileX, yy + tileY, u1, v1, u2, v2, Color);
                    }
                }

                for (int yy = y1; yy < y2; yy += tileY)
                {
                    //left
                    SpriteDrawable.AddInstance(x0, yy, x1, yy + tileY, u0, v1, u1, v2, Color);

                    //right
                    SpriteDrawable.AddInstance(x2, yy, x3, yy + tileY, u2, v1, u3, v2, Color);
                }

                for (int xx = x1; xx < x2; xx += tileX)
                {
                    //top
                    SpriteDrawable.AddInstance(xx, y0, xx + tileX, y1, u1, v0, u2, v1, Color);

                    //bottom
                    SpriteDrawable.AddInstance(xx, y2, xx + tileX, y3, u1, v2, u2, v3, Color);
                }
            }

            //top left
            SpriteDrawable.AddInstance(x0, y0, x1, y1, u0, v0, u1, v1, Color);

            //top right
            SpriteDrawable.AddInstance(x2, y0, x3, y1, u2, v0, u3, v1, Color);

            //bottom left
            SpriteDrawable.AddInstance(x0, y2, x1, y3, u0, v2, u1, v3, Color);

            //bottom right
            SpriteDrawable.AddInstance(x2, y2, x3, y3, u2, v2, u3, v3, Color);
        }

        int GetSnapWidth(int width)
        {
            var sliceWidth = sliceX2 - sliceX1;
            int snapWidth = Width - sliceWidth;

            while (snapWidth < width)
            {
                snapWidth += sliceWidth;
            }

            return snapWidth;
        }

        int GetSnapHeight(int height)
        {
            var sliceHeight = sliceY2 - sliceY1;
            int snapHeight = Width - sliceHeight;

            while (snapHeight < height)
            {
                snapHeight += sliceHeight;
            }

            return snapHeight;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the FillRect of the NineSlice.  This determines which areas are stretched or tiled when rendering the tiles.
        /// </summary>
        /// <param name="x1">The left corner of the rectangle.</param>
        /// <param name="y1">The top corner of the rectangle.</param>
        /// <param name="x2">The right corner of the rectangle.</param>
        /// <param name="y2">The bottom corner of the rectangle.</param>
        /// <returns>The NineSlice object.</returns>
        public NineSlice SetFillRect(int x1, int y1, int x2, int y2)
        {
            sliceX1 = x1;
            sliceX2 = x2;
            sliceY1 = y1;
            sliceY2 = y2;
            return this;
        }

        /// <summary>
        /// Get the FillRect of the NineSlice.  This determines which areas are stretched or tiled when rendering the tiles.
        /// </summary>
        /// <returns>The RectInt of the FillRect.</returns>
        public RectInt GetFillRect()
        {
            return new RectInt(sliceX1, sliceY1, sliceX2 - sliceX1, sliceY2 - sliceY1);
        }

        /// <summary>
        /// Set the FillRect of the NineSlice using padding values.
        /// </summary>
        /// <param name="top">How far from the top of the texture to begin the rectangle.</param>
        /// <param name="right">How far from the right of the texture to end the rectangle.</param>
        /// <param name="bottom">How far from the bottom of the texture to end the rectangle.</param>
        /// <param name="left">How far from the left of the texture to begin the rectangle.</param>
        /// <returns>The NineSlice object.</returns>
        public NineSlice SetBorderPadding(int top, int right, int bottom, int left)
        {
            var x1 = left;
            var y1 = top;
            var x2 = (int)Texture.Width - right;
            var y2 = (int)Texture.Height - bottom;
            return SetFillRect(x1, y1, x2, y2);
        }

        /// <summary>
        /// Set the FillRect of the NineSlice using padding values.
        /// </summary>
        /// <param name="padding">How far from the border of the texture to make the rectangle.</param>
        /// <returns>The NineSlice object.</returns>
        public NineSlice SetBorderPadding(int padding)
        {
            return SetBorderPadding(padding, padding, padding, padding);
        }

        /// <summary>
        /// Set the FillRect of the NineSlice using padding values.
        /// </summary>
        /// <param name="horizontal">How far horizontally from the border of the texture to make the rectangle.</param>
        /// <param name="vertical">How far horizontally from the border of the texture to make the rectangle.</param>
        /// <returns>The NineSlice object.</returns>
        public NineSlice SetBorderPadding(int horizontal, int vertical)
        {
            return SetBorderPadding(horizontal, vertical, horizontal, vertical);
        }

        #endregion

    }

    #region Enum

    public enum PanelType
    {
        Stretch,
        Tile
    }

    public enum PanelSizeMode
    {
        All,
        Inside
    }

    #endregion
}
