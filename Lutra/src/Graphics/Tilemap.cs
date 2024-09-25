using System.Collections.Generic;
using Lutra.Rendering;
using Lutra.Utility;

namespace Lutra.Graphics;

/// <summary>
/// Graphic used for loading and rendering a tilemap. Renders tiles using a SpriteDrawable.
/// </summary>
public class Tilemap : SpriteGraphic
{
    #region Public Properties

    /// <summary>
    /// The width in pixels of each tile.
    /// </summary>
    public int TileWidth { get; private set; }

    /// <summary>
    /// The height in pixels of each tile.
    /// </summary>
    public int TileHeight { get; private set; }

    /// <summary>
    /// The number of rows in the entire tilemap.
    /// </summary>
    public int TileRows { get; private set; }

    /// <summary>
    /// The number of columsn in the entire tilemap.
    /// </summary>
    public int TileColumns { get; private set; }

    /// <summary>
    /// The tile layers to render.
    /// </summary>
    public SortedDictionary<int, List<TileInfo>> TileLayers { get; private set; }

    #endregion

    #region Public Fields

    /// <summary>
    /// Determines if the X and Y positions of tiles are interpreted as pixels or tile coords.
    /// </summary>
    public bool UsePositions;

    /// <summary>
    /// The default layer name to use.
    /// </summary>
    public string DefaultLayerName = "base";

    #endregion

    #region Private Fields

    Dictionary<string, int> layerNames = new Dictionary<string, int>();

    Dictionary<int, Dictionary<int, Dictionary<int, TileInfo>>> tileTable = new Dictionary<int, Dictionary<int, Dictionary<int, TileInfo>>>();

    int
        sourceColumns,
        sourceRows;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Tilemap using the path of a texture.
    /// </summary>
    /// <param name="source">The file path to the texture to use.</param>
    /// <param name="width">The width of the Tilemap in pixels.</param>
    /// <param name="height">The height of the Tilemap in pixels.</param>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    public Tilemap(string source, int width, int height, int tileWidth, int tileHeight) : base()
    {
        SetTexture(AssetManager.GetTexture(source));
        Initialize(width, height, tileWidth, tileHeight);
    }

    /// <summary>
    /// Create a new Tilemap using the path of a texture.
    /// </summary>
    /// <param name="source">The file path to the texture to use.</param>
    /// <param name="size">The width and height of the Tilemap in pixels.</param>
    /// <param name="tileSize">The width and height of each tile in pixels.</param>
    public Tilemap(string source, int size, int tileSize) : this(source, size, size, tileSize, tileSize) { }

    /// <summary>
    /// Create a new Tilemap using a Texture.
    /// </summary>
    /// <param name="texture">The Texture to use.</param>
    /// <param name="width">The width of the Tilemap in pixels.</param>
    /// <param name="height">The height of the Tilemap in pixels.</param>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    public Tilemap(LutraTexture texture, int width, int height, int tileWidth, int tileHeight) : base()
    {
        SetTexture(texture);
        Initialize(width, height, tileWidth, tileHeight);
    }

    /// <summary>
    /// Create a new Tilemap using a Texture.
    /// </summary>
    /// <param name="texture">The Texture to use.</param>
    /// <param name="size">The width and height of the Tilemap in pixels.</param>
    /// <param name="tileSize">The width and height of each tile in pixels.</param>
    public Tilemap(LutraTexture texture, int size, int tileSize) : this(texture, size, size, tileSize, tileSize) { }

    /// <summary>
    /// Create a new Tilemap without any texture. Tiles will be solid colors instead.
    /// </summary>
    /// <param name="width">The width of the Tilemap in pixels.</param>
    /// <param name="height">The height of the Tilemap in pixels.</param>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    public Tilemap(int width, int height, int tileWidth, int tileHeight)
    {
        Initialize(width, height, tileWidth, tileHeight);
    }

    /// <summary>
    /// Create a new Tilemap without any texture. Tiles will be solid colors instead.
    /// </summary>
    /// <param name="size">The width and height of the Tilemap in pixels.</param>
    /// <param name="tileSize">The width and height of each tile in pixels.</param>
    public Tilemap(int size, int tileSize) : this(size, size, tileSize, tileSize) { }

    #endregion

    #region Private Methods

    void Initialize(int width, int height, int tileWidth, int tileHeight)
    {
        if (width < 0) throw new ArgumentOutOfRangeException("Width must be greater than 0.");
        if (height < 0) throw new ArgumentOutOfRangeException("Height must be greater than 0.");

        TileLayers = new(Comparer<int>.Create((a, b) => b - a));

        AddLayer(DefaultLayerName);

        TileWidth = tileWidth;
        TileHeight = tileHeight;

        TileColumns = (int)Util.Ceil((float)width / tileWidth);
        TileRows = (int)Util.Ceil((float)height / tileHeight);

        sourceColumns = (int)(TextureRegion.Width / tileWidth);
        sourceRows = (int)(TextureRegion.Height / tileHeight);

        Width = width;
        Height = height;
    }

    protected override void UpdateDrawable()
    {
        InitializeDrawable(Texture, WorldMatrix);

        foreach (var (_, layer) in TileLayers)
        {
            foreach (var tile in layer)
            {
                tile.tilemapColor = Color;
                tile.AddVertices(SpriteDrawable);
            }
        }
    }

    void RegisterTile(int x, int y, int layer, TileInfo tile)
    {
        if (!tileTable.ContainsKey(layer))
        {
            tileTable.Add(layer, new Dictionary<int, Dictionary<int, TileInfo>>());
        }
        if (!tileTable[layer].ContainsKey(x))
        {
            tileTable[layer].Add(x, new Dictionary<int, TileInfo>());
        }
        tileTable[layer][x].Add(y, tile);
    }

    void RemoveTile(int x, int y, int layer)
    {
        if (tileTable.ContainsKey(layer))
        {
            if (tileTable[layer].ContainsKey(x))
            {
                if (tileTable[layer][x].ContainsKey(y))
                {
                    tileTable[layer][x].Remove(y);
                }
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set a tile to a specific color.
    /// </summary>
    /// <param name="tileX">The tile's x position on the map.</param>
    /// <param name="tileY">The tile's y position on the map.</param>
    /// <param name="color">The tile's color.</param>
    /// <param name="layer">The tile's layer.</param>
    /// <returns>The TileInfo of the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, Color color, string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        // Clear the tile there first so tiles do not stack.
        ClearTile(tileX, tileY, layer);

        // Update arguments after calling other tile methods.
        if (!UsePositions)
        {
            tileX *= TileWidth;
            tileY *= TileHeight;
        }

        // Clamp tile inside tilemap.
        tileX = (int)Util.Clamp(tileX, 0, Width - TileWidth);
        tileY = (int)Util.Clamp(tileY, 0, Height - TileHeight);

        var t = new TileInfo(tileX, tileY, -1, -1, TileWidth, TileHeight, color);
        TileLayers[layerNames[layer]].Add(t);

        // Register tile for look ups.
        RegisterTile(tileX, tileY, layerNames[layer], t);

        NeedsUpdate = true;

        return t;
    }

    /// <summary>
    /// Set a tile to a specific color.
    /// </summary>
    /// <param name="tileX">The tile's x position on the map.</param>
    /// <param name="tileY">The tile's y position on the map.</param>
    /// <param name="color">The tile's color.</param>
    /// <param name="layer">The tile's layer.</param>
    /// <returns>The TileInfo of the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, Color color, Enum layer)
    {
        return SetTile(tileX, tileY, color, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set a tile to a specific tile from the source texture.
    /// </summary>
    /// <param name="tileX">The tile's X position on the map.</param>
    /// <param name="tileY">The tile's Y position on the map.</param>
    /// <param name="sourceX">The source X position from the tile map in pixels.</param>
    /// <param name="sourceY">The source Y position from the tile map in pixels.</param>
    /// <param name="layer">The tile's layer.</param>
    /// <returns>The TileInfo for the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, int sourceX, int sourceY, string layer = "")
    {
        sourceX += TextureRegion.Left;
        sourceY += TextureRegion.Top;

        if (layer == "") layer = DefaultLayerName;

        // Clear the tile there first so tiles do not stack.
        ClearTile(tileX, tileY, layer);

        // Update arguments after calling other tile methods.
        if (!UsePositions)
        {
            tileX *= TileWidth;
            tileY *= TileHeight;
        }

        tileX = (int)Util.Clamp(tileX, 0, Width - TileWidth);
        tileY = (int)Util.Clamp(tileY, 0, Height - TileHeight);

        var t = new TileInfo(tileX, tileY, sourceX, sourceY, TileWidth, TileHeight, Color.White);
        TileLayers[layerNames[layer]].Add(t);

        RegisterTile(tileX, tileY, layerNames[layer], t);

        NeedsUpdate = true;

        return t;
    }

    /// <summary>
    /// Set a tile to a specific tile from the source texture.
    /// </summary>
    /// <param name="tileX">The tile's X position on the map.</param>
    /// <param name="tileY">The tile's Y position on the map.</param>
    /// <param name="sourceX">The source X position from the tile map in pixels.</param>
    /// <param name="sourceY">The source Y position from the tile map in pixels.</param>
    /// <param name="layer">The tile's layer.</param>
    /// <returns>The TileInfo for the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, int sourceX, int sourceY, Enum layer)
    {
        return SetTile(tileX, tileY, sourceX, sourceY, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set a tile on the Tilemap to a specific tile.
    /// </summary>
    /// <param name="tileX">The X position of the tile to change.</param>
    /// <param name="tileY">The Y position of the tile to change.</param>
    /// <param name="tileIndex">The index of the tile to change to.</param>
    /// <param name="layer"></param>
    /// <returns>The TileInfo from the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, int tileIndex, string layer = "")
    {
        int sourceX = (int)(Util.TwoDeeX((int)tileIndex, (int)sourceColumns) * TileWidth);
        int sourceY = (int)(Util.TwoDeeY((int)tileIndex, (int)sourceColumns) * TileHeight);
        return SetTile(tileX, tileY, sourceX, sourceY, layer);
    }

    /// <summary>
    /// Set a tile on the Tilemap to a specific tile.
    /// </summary>
    /// <param name="tileX">The X position of the tile to change.</param>
    /// <param name="tileY">The Y position of the tile to change.</param>
    /// <param name="tileIndex">The index of the tile to change to.</param>
    /// <returns>The TileInfo from the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, int tileIndex, Enum layer)
    {
        return SetTile(tileX, tileY, tileIndex, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set a tile on the Tilemap to be flipped horizontally and/or vertically.
    /// </summary>
    /// <param name="tileX">The X position of the tile to change.</param>
    /// <param name="tileY">The Y position of the tile to change.</param>
    /// <param name="flipX">Whether the tile should be horizontally flipped.</param>
    /// <param name="flipY">Whether the tile should be vertically flipped.</param>
    /// <returns>The TileInfo from the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, bool flipX, bool flipY)
    {
        var tile = GetTile(tileX, tileY);
        tile.FlipX = flipX;
        tile.FlipY = flipY;
        return tile;
    }

    /// <summary>
    /// Set a tile on the Tilemap to be rotated by setting the flip options.
    /// </summary>
    /// <param name="tileX">The X position of the tile to change.</param>
    /// <param name="tileY">The Y position of the tile to change.</param>
    /// <param name="rotation">The angle to rotate the tile to.</param>
    /// <returns>The TileInfo from the altered tile.</returns>
    public TileInfo SetTile(int tileX, int tileY, TileRotationAndFlips rotation)
    {
        var tile = GetTile(tileX, tileY);
        tile.RotationAndFlips = rotation;
        return tile;
    }

    /// <summary>
    /// Set a rectangle area of tiles to a defined color.
    /// </summary>
    /// <param name="tileX">The X position of the tile to change.</param>
    /// <param name="tileY">The Y position of the tile to change.</param>
    /// <param name="tileWidth">The width of tiles to change.</param>
    /// <param name="tileHeight">The height of tiles to change.</param>
    /// <param name="color">The color to change the colors to.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void SetRect(int tileX, int tileY, int tileWidth, int tileHeight, Color color, string layer = "")
    {
        for (int xx = tileX; xx < tileX + tileWidth; xx++)
        {
            for (int yy = tileY; yy < tileY + tileHeight; yy++)
            {
                SetTile(xx, yy, color, layer);
            }
        }
    }

    /// <summary>
    /// Set a rectangle area of tiles to a defined color.
    /// </summary>
    /// <param name="tileX">The X position of the tile to change.</param>
    /// <param name="tileY">The Y position of the tile to change.</param>
    /// <param name="tileWidth">The width of tiles to change.</param>
    /// <param name="tileHeight">The height of tiles to change.</param>
    /// <param name="color">The color to change the colors to.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void SetRect(int tileX, int tileY, int tileWidth, int tileHeight, Color color, Enum layer)
    {
        SetRect(tileX, tileY, tileWidth, tileHeight, color, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set a rectangle of tiles to a tile defined by texture coordinates.
    /// </summary>
    /// <param name="tileX">The X position of the rectangle to change.</param>
    /// <param name="tileY">The Y position of the rectangle to change.</param>
    /// <param name="tileWidth">The width of tiles to change.</param>
    /// <param name="tileHeight">The height of tiles to change.</param>
    /// <param name="sourceX">The X position in the source Texture to use to draw the tiles.</param>
    /// <param name="sourceY">The Y position in the source Texture to use to draw the tiles.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void SetRect(int tileX, int tileY, int tileWidth, int tileHeight, int sourceX, int sourceY, string layer = "")
    {
        for (int xx = tileX; xx < tileX + tileWidth; xx++)
        {
            for (int yy = tileY; yy < tileY + tileHeight; yy++)
            {
                SetTile(xx, yy, sourceX, sourceY, layer);
            }
        }
    }

    /// <summary>
    /// Set a rectangle of tiles to a tile defined by texture coordinates.
    /// </summary>
    /// <param name="tileX">The X position of the rectangle to change.</param>
    /// <param name="tileY">The Y position of the rectangle to change.</param>
    /// <param name="tileWidth">The width of tiles to change.</param>
    /// <param name="tileHeight">The height of tiles to change.</param>
    /// <param name="sourceX">The X position in the source Texture to use to draw the tiles.</param>
    /// <param name="sourceY">The Y position in the source Texture to use to draw the tiles.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void SetRect(int tileX, int tileY, int tileWidth, int tileHeight, int sourceX, int sourceY, Enum layer)
    {
        SetRect(tileX, tileY, tileWidth, tileHeight, sourceX, sourceY, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set a rectangle of tiles to a tile defined by an index.
    /// </summary>
    /// <param name="tileX">The X position of the rectangle to change.</param>
    /// <param name="tileY">The Y position of the rectangle to change.</param>
    /// <param name="tileWidth">The width of tiles to change.</param>
    /// <param name="tileHeight">The height of tiles to change.</param>
    /// <param name="tileIndex">The index of the tile to change the tiles to.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void SetRect(int tileX, int tileY, int tileWidth, int tileHeight, int tileIndex, string layer = "")
    {
        for (int xx = tileX; xx < tileX + tileWidth; xx++)
        {
            for (int yy = tileY; yy < tileY + tileHeight; yy++)
            {
                SetTile(xx, yy, tileIndex, layer);
            }
        }
    }

    /// <summary>
    /// Set a rectangle of tiles to a tile defined by an index.
    /// </summary>
    /// <param name="tileX">The X position of the rectangle to change.</param>
    /// <param name="tileY">The Y position of the rectangle to change.</param>
    /// <param name="tileWidth">The width of tiles to change.</param>
    /// <param name="tileHeight">The height of tiles to change.</param>
    /// <param name="tileIndex">The index of the tile to change the tiles to.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void SetRect(int tileX, int tileY, int tileWidth, int tileHeight, int tileIndex, Enum layer)
    {
        SetRect(tileX, tileY, tileWidth, tileHeight, tileIndex, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set all tiles of a specific layer.
    /// </summary>
    /// <param name="tileIndex">The index of the tile to change the tiles to.</param>
    /// <param name="layer">The layer to change.</param>
    public void SetLayer(int tileIndex, string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        SetRect(0, 0, TileColumns, TileRows, tileIndex, layer);
    }

    /// <summary>
    /// Set all tiles of a specific layer.
    /// </summary>
    /// <param name="sourceX">The X position in the source Texture to use to draw the tiles.</param>
    /// <param name="sourceY">The Y position in the source Texture to use to draw the tiles.</param>
    /// <param name="layer">The layer to change.</param>
    public void SetLayer(int sourceX, int sourceY, string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        SetRect(0, 0, TileColumns, TileRows, sourceX, sourceY, layer);
    }

    /// <summary>
    /// Set all tiles of a specific layer.
    /// </summary>
    /// <param name="color">The color to change the tile to.</param>
    /// <param name="layer">The layer to change.</param>
    public void SetLayer(Color color, string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        SetRect(0, 0, TileColumns, TileRows, color, layer);
    }

    /// <summary>
    /// Set all tiles of a specific layer.
    /// </summary>
    /// <param name="tileIndex">The index of the tile to change the tiles to.</param>
    /// <param name="layer">The layer to change.</param>
    public void SetLayer(int tileIndex, Enum layer)
    {
        SetLayer(tileIndex, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set all tiles of a specific layer.
    /// </summary>
    /// <param name="sourceX">The X position in the source Texture to use to draw the tiles.</param>
    /// <param name="sourceY">The Y position in the source Texture to use to draw the tiles.</param>
    /// <param name="layer">The layer to change.</param>
    public void SetLayer(int sourceX, int sourceY, Enum layer)
    {
        SetLayer(sourceX, sourceY, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Set all tiles of a specific layer.
    /// </summary>
    /// <param name="color">The color to change the tile to.</param>
    /// <param name="layer">The layer to change.</param>
    public void SetLayer(Color color, Enum layer)
    {
        SetLayer(color, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Get the TileInfo of a specific tile on the tilemap.
    /// </summary>
    /// <param name="tileX">The X position of the tile to retrieve.</param>
    /// <param name="tileY">The Y position of the tile to retrieve.</param>
    /// <param name="layer">The layer to search through for the tile.</param>
    /// <returns>The TileInfo for the found tile.</returns>
    public TileInfo GetTile(int tileX, int tileY, string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        if (!UsePositions)
        {
            tileX *= TileWidth;
            tileY *= TileHeight;
        }

        tileX = (int)Util.Clamp(tileX, 0, Width - TileWidth);
        tileY = (int)Util.Clamp(tileY, 0, Height - TileHeight);

        var layerDepth = layerNames[layer];
        if (!tileTable.ContainsKey(layerDepth))
        {
            return null;
        }

        if (!tileTable[layerDepth].ContainsKey(tileX))
        {
            return null;
        }

        if (!tileTable[layerDepth][tileX].ContainsKey(tileY))
        {
            return null;
        }

        return tileTable[layerDepth][tileX][tileY];
    }

    /// <summary>
    /// Get the TileInfo of a specific tile on the tilemap.
    /// </summary>
    /// <param name="tileX">The X position of the tile to retrieve.</param>
    /// <param name="tileY">The Y position of the tile to retrieve.</param>
    /// <param name="layer">The layer to search through for the tile.</param>
    /// <returns>The TileInfo for the found tile.</returns>
    public TileInfo GetTile(int tileX, int tileY, Enum layer)
    {
        return GetTile(tileX, tileY, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Get the layer name for a specific layer on the tilemap.
    /// </summary>
    /// <param name="layer">The layer depth id.</param>
    /// <returns>The string name of the layer.</returns>
    public string LayerName(int layer)
    {
        foreach (var l in layerNames)
        {
            if (l.Value == layer)
            {
                return l.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the layer depth of a layer on the tilemap by name.
    /// </summary>
    /// <param name="layer">The string layer name.</param>
    /// <returns>The layer depth id.</returns>
    public int LayerDepth(string layer)
    {
        return layerNames[layer];
    }

    /// <summary>
    /// Get the layer depth of a layer on the tilemap by enum value.
    /// </summary>
    /// <param name="layer">The enum value layer.</param>
    /// <returns>The layer depth id.</returns>
    public int LayerDepth(Enum layer)
    {
        return LayerDepth(Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Load the tilemap with a color based on a string.
    /// </summary>
    /// <param name="source">The string data to load.</param>
    /// <param name="color">The color to fill occupied tiles with.</param>
    /// <param name="empty">The character that represents an empty tile.</param>
    /// <param name="filled">The character that represents a filled tile.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void LoadString(string source, Color color, char empty = '0', char filled = '1', string layer = "")
    {
        int xx = 0, yy = 0;

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] != empty && source[i] != filled) continue;

            if (xx == TileColumns)
            {
                xx = 0;
                yy++;
            }

            if (source[i] == filled)
            {
                SetTile(xx, yy, color, layer);
            }

            xx++;
        }
    }

    /// <summary>
    /// Load the tilemap with a color based on a string.
    /// </summary>
    /// <param name="source">The string data to load.</param>
    /// <param name="color">The color to fill occupied tiles with.</param>
    /// <param name="empty">The character that represents an empty tile.</param>
    /// <param name="filled">The character that represents a filled tile.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void LoadString(string source, Color color, char empty, char filled, Enum layer)
    {
        LoadString(source, color, empty, filled, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Load the tilemap from a CSV formatted string.
    /// </summary>
    /// <param name="str">The string data to load.</param>
    /// <param name="columnSep">The character that separates columns in the CSV.</param>
    /// <param name="rowSep">The character that separates rows in the CSV.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void LoadCSV(string str, char columnSep = ',', char rowSep = '\n', string layer = "")
    {
        bool u = UsePositions;
        UsePositions = false;

        string[] row = str.Split(rowSep);
        int rows = row.Length;
        string[] col;
        int cols;
        int x;
        int y;

        for (y = 0; y < rows; y++)
        {
            if (row[y] == "")
            {
                continue;
            }

            col = row[y].Split(columnSep);
            cols = col.Length;
            for (x = 0; x < cols; x++)
            {
                if (col[x].Equals("") || Convert.ToInt32(col[x]) < 0)
                {
                    continue;
                }

                SetTile(x, y, Convert.ToInt16(col[x]), layer);
            }
        }

        UsePositions = u;
    }

    /// <summary>
    /// Load the tilemap from a CSV formatted string.
    /// </summary>
    /// <param name="str">The string data to load.</param>
    /// <param name="columnSep">The character that separates columns in the CSV.</param>
    /// <param name="rowSep">The character that separates rows in the CSV.</param>
    /// <param name="layer">The layer to place the tiles on.</param>
    public void LoadCSV(string str, char columnSep, char rowSep, Enum layer)
    {
        LoadCSV(str, columnSep, rowSep, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Remove a tile from the tilemap.
    /// </summary>
    /// <param name="tileX">The tile's X position on the map.</param>
    /// <param name="tileY">The tile's Y position on the map.</param>
    /// <param name="layer">The tile's layer.</param>
    /// <returns>The TileInfo for the cleared tile.</returns>
    public TileInfo ClearTile(int tileX, int tileY, string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        var t = GetTile(tileX, tileY, layer);

        if (!UsePositions)
        {
            tileX *= TileWidth;
            tileY *= TileHeight;
        }

        tileX = (int)Util.Clamp(tileX, 0, Width - TileWidth);
        tileY = (int)Util.Clamp(tileY, 0, Height - TileHeight);

        RemoveTile(tileX, tileY, layerNames[layer]);

        if (t != null)
        {
            TileLayers[layerNames[layer]].Remove(t);
        }

        NeedsUpdate = true;

        return t;
    }

    /// <summary>
    /// Remove a tile from the tilemap.
    /// </summary>
    /// <param name="tileX">The tile's X position on the map.</param>
    /// <param name="tileY">The tile's Y position on the map.</param>
    /// <param name="layer">The tile's layer.</param>
    /// <returns>The TileInfo for the cleared tile.</returns>
    public TileInfo ClearTile(int tileX, int tileY, Enum layer)
    {
        return ClearTile(tileX, tileY, Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Clear all tiles on a specific layer.
    /// </summary>
    /// <param name="layer">The string layer name.</param>
    public void ClearLayer(string layer = "")
    {
        if (layer == "") layer = DefaultLayerName;

        TileLayers[layerNames[layer]].Clear();

        NeedsUpdate = true;
    }

    /// <summary>
    /// Clear all tiles on a specific layer.
    /// </summary>
    /// <param name="layer">The enum value layer.</param>
    public void ClearLayer(Enum layer)
    {
        ClearLayer(Util.EnumValueToString(layer));
    }

    /// <summary>
    /// Clear all tiles on all layers.
    /// </summary>
    public void ClearAll()
    {
        foreach (var kv in TileLayers)
        {
            kv.Value.Clear();
        }

        NeedsUpdate = true;
    }

    /// <summary>
    /// Clear all tiles inside a specified rectangle.
    /// </summary>
    /// <param name="tileX">The X position of the rectangle to clear.</param>
    /// <param name="tileY">The Y position of the rectangle to clear.</param>
    /// <param name="tileWidth">The width of tiles to clear.</param>
    /// <param name="tileHeight">The height of tiles to clear.</param>
    /// <param name="layer">The layer to clear tiles from.</param>
    public void ClearRect(int tileX, int tileY, int tileWidth, int tileHeight, string layer = "")
    {
        for (int xx = tileX; xx < tileX + tileWidth; xx++)
        {
            for (int yy = tileY; yy < tileY + tileHeight; yy++)
            {
                ClearTile(xx, yy, layer);
            }
        }
    }

    /// <summary>
    /// Add a new layer to the Tilemap.
    /// </summary>
    /// <param name="name">The string name of the layer.</param>
    /// <param name="depth">The depth of the tiles.</param>
    /// <returns>The depth id of the layer.</returns>
    public int AddLayer(string name, int depth = 0)
    {
        layerNames.Add(name, depth);
        TileLayers.Add(depth, new List<TileInfo>());

        return TileLayers.Count - 1;
    }

    /// <summary>
    /// Add a new layer to the Tilemap.
    /// </summary>
    /// <param name="name">The enum value of the layer.</param>
    /// <param name="depth">The depth of the tiles.</param>
    /// <returns>The depth id of the layer.</returns>
    public int AddLayer(Enum name, int depth = 0)
    {
        return AddLayer(Util.EnumValueToString(name), depth);
    }

    /// <summary>
    /// Remove a layer from the Tilemap and delete that layer's tiles.
    /// </summary>
    /// <param name="name">The name of the layer to delete.</param>
    public void RemoveLayer(string name)
    {
        ClearLayer(name);
        if (name != DefaultLayerName)
        {
            TileLayers.Remove(layerNames[name]);
            layerNames.Remove(name);
        }
    }

    /// <summary>
    /// Remove a layer from the Tilemap and delete that layer's tiles.
    /// </summary>
    /// <param name="name">The name of the layer to delete.</param>
    public void RemoveLayer(Enum name)
    {
        RemoveLayer(Util.EnumValueToString(name));
    }

    /// <summary>
    /// Check if a layer exists.
    /// </summary>
    /// <param name="name">The string name of the layer.</param>
    /// <returns>True if the layer exists.</returns>
    public bool LayerExists(string name)
    {
        return TileLayers.ContainsKey(layerNames[name]);
    }

    /// <summary>
    /// Check if a layer exists.
    /// </summary>
    /// <param name="name">The enum value of the layer.</param>
    /// <returns>True if the layer exists.</returns>
    public bool LayerExists(Enum name)
    {
        return LayerExists(Util.EnumValueToString(name));
    }

    /// <summary>
    /// Merges another tilemap into this one.
    /// </summary>
    /// <param name="other">The tilemap to merge into this one.</param>
    /// <param name="above">True if the other tilemap's base layer should be above this one's base layer.</param>
    public void MergeTilemap(Tilemap other, string layerPrefix = "", int layerOffset = -1)
    {
        foreach (var layer in other.TileLayers)
        {
            AddLayer(layerPrefix + other.LayerName(layer.Key), layer.Key + layerOffset);
            TileLayers[layer.Key + layerOffset] = new List<TileInfo>(other.TileLayers[layer.Key]);
        }
    }

    /// <summary>
    /// Get the list of tiles on a specific layer.
    /// </summary>
    /// <param name="layerName">The name of the layer.</param>
    /// <returns>A list of tiles on that layer.</returns>
    public List<TileInfo> GetTiles(string layerName)
    {
        return TileLayers[LayerDepth(layerName)];
    }

    /// <summary>
    /// Get the list of tiles on a specific layer.
    /// </summary>
    /// <param name="layerDepth"></param>
    /// <returns>A list of tiles on that layer.</returns>
    public List<TileInfo> GetTiles(int layerDepth)
    {
        return TileLayers[layerDepth];
    }

    /// <summary>
    /// Get the list of tiles on a specific layer.
    /// </summary>
    /// <param name="layerName">The enum value of the layer.</param>
    /// <returns>A list of tiles on that layer.</returns>
    public List<TileInfo> GetTiles(Enum layerName)
    {
        return TileLayers[LayerDepth(layerName)];
    }

    /// <summary>
    /// Get the index of a specific tile on the source Texture.
    /// </summary>
    /// <param name="tile">The tile to get the index of.</param>
    /// <returns>The index of the tile.</returns>
    public int GetTileIndex(TileInfo tile)
    {
        return tile.GetIndex(this);
    }

    #endregion
}
