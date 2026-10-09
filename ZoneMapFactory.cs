using Microsoft.Xna.Framework;

namespace ValleQuebrado.World;

/// <summary>
/// Construye el mapa de cualquier zona de WorldGraph. Cada salida (Warp.FromTile)
/// queda marcada con un tile brillante y conectada por camino con el centro del mapa.
/// </summary>
public static class ZoneMapFactory
{
    public static TileMap Build(LocationId id)
    {
        TileMap map = id == LocationId.Farm ? FarmMapFactory.Build() : BuildGeneric(id);

        foreach (Warp warp in WorldGraph.WarpsFrom(id))
            if (map.Get(warp.FromTile.X, warp.FromTile.Y) != TileType.CaveEntrance)
                map.Set(warp.FromTile.X, warp.FromTile.Y, TileType.Exit);

        return map;
    }

    private static (TileType Type, int Density) Obstacles(LocationId id, LocationKind kind) => id switch
    {
        LocationId.SpectralForest => (TileType.Tree, 3),
        LocationId.BrokenMountains => (TileType.Rock, 5),
        LocationId.GiantsPass => (TileType.Rock, 6),
        LocationId.Village => (TileType.Tree, 14),
        LocationId.EchoLake => (TileType.Tree, 9),
        LocationId.AshDesert => (TileType.CaveWall, 12),
        LocationId.Docks => (TileType.CaveWall, 30),
        _ => kind == LocationKind.Interior ? (TileType.CaveWall, 20) : (TileType.CaveWall, 7)
    };

    private static TileMap BuildGeneric(LocationId id)
    {
        LocationDef def = WorldGraph.Locations[id];
        List<Point> exits = WorldGraph.WarpsFrom(id).Select(w => w.FromTile).ToList();
        bool outdoor = def.Kind == LocationKind.Outdoor;

        (int minW, int minH) = def.Kind switch
        {
            LocationKind.Interior => (30, 20),
            LocationKind.Dungeon => (48, 32),
            _ => (70, 50)
        };
        int width = Math.Max(minW, (exits.Count > 0 ? exits.Max(p => p.X) : 0) + 8);
        int height = Math.Max(minH, (exits.Count > 0 ? exits.Max(p => p.Y) : 0) + 8);

        bool sandy = id == LocationId.AshDesert || id == LocationId.Docks;
        TileType ground = !outdoor ? TileType.Floor : sandy ? TileType.Sand : TileType.Grass;
        TileType road = ground == TileType.Floor ? TileType.Floor : TileType.Path;
        (TileType obstacle, int density) = Obstacles(id, def.Kind);

        var map = new TileMap(width, height, ground);
        var reserved = new bool[width, height];

        // Borde del mapa.
        for (int x = 0; x < width; x++) { map.Set(x, 0, obstacle); map.Set(x, height - 1, obstacle); }
        for (int y = 0; y < height; y++) { map.Set(0, y, obstacle); map.Set(width - 1, y, obstacle); }

        // Decoración propia de cada zona (antes de los caminos, que pueden cruzarla como puentes).
        switch (id)
        {
            case LocationId.Village:
                foreach (Point h in new[] { new Point(10, 10), new Point(22, 8), new Point(50, 10),
                                            new Point(60, 16), new Point(10, 40), new Point(25, 44) })
                    BuildHouse(map, reserved, h.X, h.Y);
                break;
            case LocationId.EchoLake:
                Ellipse(map, 48, 32, 14, 9, TileType.Water);
                break;
            case LocationId.Docks:
                for (int y = height - 16; y < height - 1; y++)
                    for (int x = 1; x < width - 1; x++)
                        map.Set(x, y, TileType.Water);
                break;
            case LocationId.Sewers:
                for (int x = 2; x < width - 2; x++) { map.Set(x, 12, TileType.Water); map.Set(x, 13, TileType.Water); }
                break;
            case LocationId.GiantsPass:
                for (int y = 1; y < height - 1; y++)
                    for (int x = 30; x <= 38; x++)
                        map.Set(x, y, TileType.Water);
                break;
        }

        // Caminos desde cada salida hasta el centro.
        var center = new Point(width / 2, height / 2);
        foreach (Point e in exits)
            Carve(map, reserved, e, center, road, e.Y <= 0 || e.Y >= height - 1);
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                Paint(map, reserved, center.X + dx, center.Y + dy, road);

        // Obstáculos repartidos (árboles, rocas, pilares) sin tapar caminos.
        for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                if (reserved[x, y] || map.Get(x, y) != ground) continue;
                if (Hash(x + (int)id * 131, y) % density != 0) continue;
                map.Set(x, y, obstacle);
            }

        return map;
    }

    private static void BuildHouse(TileMap m, bool[,] reserved, int x, int y)
    {
        for (int dx = 0; dx < 5; dx++)
        {
            m.Set(x + dx, y, TileType.Roof);
            m.Set(x + dx, y + 1, TileType.Roof);
            m.Set(x + dx, y + 2, TileType.Wall);
            m.Set(x + dx, y + 3, TileType.Wall);
        }
        m.Set(x + 2, y + 3, TileType.Door);
        m.Set(x + 2, y + 4, TileType.Path);

        for (int ry = y - 1; ry <= y + 5; ry++)
            for (int rx = x - 1; rx <= x + 5; rx++)
                if (m.InBounds(rx, ry))
                    reserved[rx, ry] = true;
    }

    private static void Ellipse(TileMap m, int cx, int cy, int rx, int ry, TileType t)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                float dx = (x - cx) / (float)rx, dy = (y - cy) / (float)ry;
                if (dx * dx + dy * dy <= 1f)
                    m.Set(x, y, t);
            }
    }

    private static void Carve(TileMap map, bool[,] reserved, Point from, Point to, TileType road, bool verticalFirst)
    {
        int x = from.X, y = from.Y;
        if (verticalFirst)
        {
            while (y != to.Y) { Paint(map, reserved, x, y, road); y += Math.Sign(to.Y - y); }
            while (x != to.X) { Paint(map, reserved, x, y, road); x += Math.Sign(to.X - x); }
        }
        else
        {
            while (x != to.X) { Paint(map, reserved, x, y, road); x += Math.Sign(to.X - x); }
            while (y != to.Y) { Paint(map, reserved, x, y, road); y += Math.Sign(to.Y - y); }
        }
        Paint(map, reserved, x, y, road);
    }

    private static void Paint(TileMap map, bool[,] reserved, int x, int y, TileType road)
    {
        map.Set(x, y, road);
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                if (map.InBounds(x + dx, y + dy))
                    reserved[x + dx, y + dy] = true;
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
}
