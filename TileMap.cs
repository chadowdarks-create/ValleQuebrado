using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ValleQuebrado.World;

public enum TileType : byte
{
    Grass, Path, Soil, Water, Tree, Fence, Wall, Roof, Door, Rock, CaveEntrance,
    Floor, CaveWall, Sand, Exit
}

/// <summary>Cuadrícula de tiles con colisión y dibujo procedural (sin imágenes aún).</summary>
public sealed class TileMap
{
    public const int TileSize = 32;

    private readonly TileType[,] _tiles;

    public int Width { get; }
    public int Height { get; }
    public int PixelWidth => Width * TileSize;
    public int PixelHeight => Height * TileSize;

    public TileMap(int width, int height, TileType fill = TileType.Grass)
    {
        Width = width;
        Height = height;
        _tiles = new TileType[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _tiles[x, y] = fill;
    }

    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

    /// <summary>Fuera del mapa se considera sólido para que el jugador no se escape.</summary>
    public TileType Get(int x, int y) => InBounds(x, y) ? _tiles[x, y] : TileType.Tree;

    public void Set(int x, int y, TileType type)
    {
        if (InBounds(x, y))
            _tiles[x, y] = type;
    }

    public static bool IsSolid(TileType t) => t switch
    {
        TileType.Water or TileType.Tree or TileType.Fence or TileType.Wall
            or TileType.Roof or TileType.Rock or TileType.CaveWall => true,
        _ => false
    };

    /// <summary>Indica si un rectángulo (en píxeles del mundo) toca algún tile sólido.</summary>
    public bool IsBlocked(Rectangle area)
    {
        int x0 = (int)MathF.Floor(area.Left / (float)TileSize);
        int y0 = (int)MathF.Floor(area.Top / (float)TileSize);
        int x1 = (int)MathF.Floor((area.Right - 1) / (float)TileSize);
        int y1 = (int)MathF.Floor((area.Bottom - 1) / (float)TileSize);

        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                if (IsSolid(Get(x, y)))
                    return true;
        return false;
    }

    /// <summary>Dibuja únicamente los tiles visibles por la cámara.</summary>
    public void Draw(SpriteBatch sb, Texture2D px, Rectangle visible, float time)
    {
        int x0 = Math.Max(0, visible.Left / TileSize);
        int y0 = Math.Max(0, visible.Top / TileSize);
        int x1 = Math.Min(Width - 1, visible.Right / TileSize);
        int y1 = Math.Min(Height - 1, visible.Bottom / TileSize);

        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                DrawTile(sb, px, x, y, _tiles[x, y], time);
    }

    private static int Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 73856093 ^ y * 19349663;
            h ^= h >> 13;
            return h & 0x7fffffff;
        }
    }

    private static void Fill(SpriteBatch sb, Texture2D px, int tx, int ty, int ox, int oy, int w, int h, Color c)
        => sb.Draw(px, new Rectangle(tx * TileSize + ox, ty * TileSize + oy, w, h), c);

    private static void DrawTile(SpriteBatch sb, Texture2D px, int x, int y, TileType type, float time)
    {
        int h = Hash(x, y);
        int shade = (h % 3 - 1) * 5;

        switch (type)
        {
            case TileType.Grass:
                DrawGrass(sb, px, x, y, h, shade);
                break;

            case TileType.Path:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(176 + shade, 146 + shade, 98 + shade));
                if (h % 6 == 0)
                    Fill(sb, px, x, y, h % 24 + 3, (h / 5) % 24 + 3, 3, 3, new Color(150, 122, 80));
                break;

            case TileType.Soil:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(112 + shade, 78 + shade, 52 + shade));
                for (int oy = 6; oy < TileSize; oy += 8)
                    Fill(sb, px, x, y, 0, oy, TileSize, 2, new Color(88, 58, 38));
                break;

            case TileType.Water:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(48, 104, 184));
                int s = (int)(time * 10f + x * 7 + y * 3) % 40;
                if (s < 24)
                    Fill(sb, px, x, y, s, 8 + (h % 3) * 8, 8, 2, new Color(110, 160, 220));
                break;

            case TileType.Tree:
                DrawGrass(sb, px, x, y, h, shade);
                Fill(sb, px, x, y, 13, 16, 6, 16, new Color(104, 70, 40));
                Fill(sb, px, x, y, 2, 0, 28, 22, new Color(34, 98, 46));
                Fill(sb, px, x, y, 6, 3, 12, 8, new Color(54, 128, 62));
                break;

            case TileType.Fence:
                DrawGrass(sb, px, x, y, h, shade);
                Fill(sb, px, x, y, 0, 12, TileSize, 4, new Color(160, 112, 64));
                Fill(sb, px, x, y, 0, 22, TileSize, 4, new Color(160, 112, 64));
                Fill(sb, px, x, y, 2, 8, 4, 22, new Color(130, 90, 50));
                Fill(sb, px, x, y, 26, 8, 4, 22, new Color(130, 90, 50));
                break;

            case TileType.Wall:
                DrawWall(sb, px, x, y);
                break;

            case TileType.Roof:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(170, 60, 56));
                for (int oy = 7; oy < TileSize; oy += 8)
                    Fill(sb, px, x, y, 0, oy, TileSize, 2, new Color(138, 44, 42));
                break;

            case TileType.Door:
                DrawWall(sb, px, x, y);
                Fill(sb, px, x, y, 6, 4, 20, 28, new Color(80, 52, 32));
                Fill(sb, px, x, y, 21, 18, 3, 3, new Color(230, 190, 70));
                break;

            case TileType.Rock:
                DrawGrass(sb, px, x, y, h, shade);
                Fill(sb, px, x, y, 2, 6, 28, 24, new Color(120, 120, 128));
                Fill(sb, px, x, y, 4, 8, 10, 6, new Color(152, 152, 160));
                Fill(sb, px, x, y, 2, 26, 28, 4, new Color(88, 88, 96));
                break;

            case TileType.Floor:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(96 + shade, 96 + shade, 108 + shade));
                Fill(sb, px, x, y, 0, 0, TileSize, 1, new Color(78, 78, 90));
                Fill(sb, px, x, y, 0, 0, 1, TileSize, new Color(78, 78, 90));
                break;

            case TileType.CaveWall:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(44, 40, 54));
                Fill(sb, px, x, y, 0, 0, TileSize, 5, new Color(70, 64, 82));
                if (h % 4 == 0)
                    Fill(sb, px, x, y, h % 20 + 4, 10 + (h / 3) % 14, 2, 8, new Color(28, 26, 36));
                break;

            case TileType.Sand:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(216 + shade, 190 + shade, 120 + shade));
                if (h % 6 == 0)
                    Fill(sb, px, x, y, h % 24 + 3, (h / 5) % 24 + 3, 3, 2, new Color(190, 160, 96));
                break;

            case TileType.Exit:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(70, 58, 44));
                float pulse = 0.55f + 0.25f * MathF.Sin(time * 3f + x);
                Fill(sb, px, x, y, 4, 4, 24, 24, new Color(255, 226, 120) * (pulse * 0.5f));
                Fill(sb, px, x, y, 10, 10, 12, 12, new Color(255, 236, 160) * pulse);
                break;

            case TileType.CaveEntrance:
                Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(60, 60, 66));
                Fill(sb, px, x, y, 6, 8, 20, 24, new Color(14, 10, 18));
                break;
        }
    }

    private static void DrawGrass(SpriteBatch sb, Texture2D px, int x, int y, int h, int shade)
    {
        Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(76 + shade, 140 + shade, 62 + shade));
        if (h % 5 == 0)
            Fill(sb, px, x, y, h % 24 + 4, (h / 7) % 24 + 4, 2, 4, new Color(58, 112, 48));
    }

    private static void DrawWall(SpriteBatch sb, Texture2D px, int x, int y)
    {
        Fill(sb, px, x, y, 0, 0, TileSize, TileSize, new Color(150, 104, 70));
        for (int oy = 7; oy < TileSize; oy += 8)
            Fill(sb, px, x, y, 0, oy, TileSize, 1, new Color(120, 80, 52));
    }
}
