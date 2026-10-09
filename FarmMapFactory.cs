using Microsoft.Xna.Framework;

namespace ValleQuebrado.World;

/// <summary>
/// Genera el mapa de la Granja (80x50). Las salidas coinciden con los Warps de WorldGraph:
/// (30,12) Sótano Roto, (79,20) Aldea y (0,25) Bosque Espectral.
/// </summary>
public static class FarmMapFactory
{
    public static readonly Point SpawnTile = new(12, 23);

    public static TileMap Build()
    {
        var m = new TileMap(80, 50);

        // Borde de árboles (con huecos en las salidas).
        for (int x = 0; x < 80; x++) { m.Set(x, 0, TileType.Tree); m.Set(x, 49, TileType.Tree); }
        for (int y = 0; y < 50; y++) { m.Set(0, y, TileType.Tree); m.Set(79, y, TileType.Tree); }

        // Caminos principales.
        for (int x = 0; x <= 40; x++) m.Set(x, 25, TileType.Path);     // oeste -> centro
        for (int y = 20; y <= 25; y++) m.Set(40, y, TileType.Path);    // sube a la aldea
        for (int x = 40; x <= 79; x++) m.Set(x, 20, TileType.Path);    // hacia la aldea
        for (int y = 13; y <= 25; y++) m.Set(30, y, TileType.Path);    // hacia el sótano
        for (int y = 22; y <= 25; y++) m.Set(12, y, TileType.Path);    // puerta de la casa
        m.Set(21, 23, TileType.Path); m.Set(21, 24, TileType.Path);    // entrada al huerto

        // Casa (paredes y techo) con puerta en (12,21).
        for (int x = 10; x <= 15; x++)
        {
            m.Set(x, 17, TileType.Roof); m.Set(x, 18, TileType.Roof);
            m.Set(x, 19, TileType.Wall); m.Set(x, 20, TileType.Wall); m.Set(x, 21, TileType.Wall);
        }
        m.Set(12, 21, TileType.Door);

        // Huerto con tierra labrable y cerca.
        for (int y = 14; y <= 22; y++)
            for (int x = 17; x <= 26; x++)
                m.Set(x, y, TileType.Soil);
        for (int x = 16; x <= 27; x++) { m.Set(x, 13, TileType.Fence); m.Set(x, 23, TileType.Fence); }
        for (int y = 13; y <= 23; y++) { m.Set(16, y, TileType.Fence); m.Set(27, y, TileType.Fence); }
        m.Set(21, 23, TileType.Path);

        // Estanque.
        for (int y = 28; y <= 40; y++)
            for (int x = 46; x <= 64; x++)
            {
                float dx = (x - 55) / 9f, dy = (y - 34) / 5f;
                if (dx * dx + dy * dy <= 1f)
                    m.Set(x, y, TileType.Water);
            }

        // Entrada al Sótano Roto: rocas alrededor de la cueva.
        for (int x = 29; x <= 31; x++) { m.Set(x, 10, TileType.Rock); m.Set(x, 11, TileType.Rock); }
        m.Set(29, 12, TileType.Rock); m.Set(31, 12, TileType.Rock);
        m.Set(30, 12, TileType.CaveEntrance);

        // Árboles repartidos (sin tapar caminos ni la zona de inicio).
        for (int y = 2; y < 48; y++)
            for (int x = 2; x < 78; x++)
            {
                if (m.Get(x, y) != TileType.Grass) continue;
                if (Hash(x, y) % 9 != 0) continue;
                if (NearPath(m, x, y)) continue;
                m.Set(x, y, TileType.Tree);
            }

        return m;
    }

    private static bool NearPath(TileMap m, int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                TileType t = m.Get(x + dx, y + dy);
                if (t == TileType.Path || t == TileType.Door || t == TileType.CaveEntrance)
                    return true;
            }
        return false;
    }

    private static int Hash(int x, int y)
    {
        unchecked
        {
            int h = x * 73856093 ^ y * 40503;
            h ^= h >> 11;
            return h & 0x7fffffff;
        }
    }
}
