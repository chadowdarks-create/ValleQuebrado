using Microsoft.Xna.Framework;

namespace ValleQuebrado.World;

public enum LocationId
{
    Farm, Village, Sewers, BrokenBasement,
    SoulMines, SkullCrypt, VoidFortress,
    BrokenMountains, GiantsPass, SpectralForest,
    Docks, SubmarineCaves, AshDesert, EchoLake
}

public enum LocationKind { Outdoor, Interior, Dungeon }

/// <summary>Estado de progreso consultado por las condiciones de acceso.</summary>
public sealed class WorldState
{
    public bool HasDungeonPermit { get; set; }
    public int GuildRank { get; set; }
    public bool HasSewerKey { get; set; }
    public bool GiantBridgeRepaired { get; set; }
    public int HighestBossFloorDefeated { get; set; }
    public int GeneralsDefeated { get; set; }
    public bool LowTide { get; set; }
    public int Gold { get; set; }
}

/// <summary>Transición entre dos zonas, con coordenadas y requisitos de acceso.</summary>
public sealed class Warp
{
    public LocationId From { get; init; }
    public LocationId To { get; init; }
    public Point FromTile { get; init; }
    public Point ToTile { get; init; }
    public Func<WorldState, bool> IsOpen { get; init; } = _ => true;
    public string LockedMessage { get; init; } = "Algo te impide pasar.";
    public int GoldCost { get; init; }
}

/// <summary>Datos estáticos de una zona del mundo.</summary>
public sealed class LocationDef
{
    public LocationId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public LocationKind Kind { get; init; }
    public string TmxPath { get; init; } = string.Empty;
    public bool IsProcedural { get; init; }
}

/// <summary>Catálogo de las 14 zonas y sus conexiones bidireccionales.</summary>
public static class WorldGraph
{
    public static IReadOnlyDictionary<LocationId, LocationDef> Locations { get; }
    public static IReadOnlyList<Warp> Warps { get; }

    static WorldGraph()
    {
        var locations = new Dictionary<LocationId, LocationDef>();
        var warps = new List<Warp>();

        Add(LocationId.Farm, "Granja", LocationKind.Outdoor, "Maps/Farm");
        Add(LocationId.Village, "Aldea Valle Quebrado", LocationKind.Outdoor, "Maps/Village");
        Add(LocationId.Sewers, "Alcantarillas", LocationKind.Interior, "Maps/Sewers");
        Add(LocationId.BrokenBasement, "Sótano Roto", LocationKind.Dungeon, "Maps/BrokenBasement", true);
        Add(LocationId.SoulMines, "Minas de Almas", LocationKind.Dungeon, "Maps/SoulMines", true);
        Add(LocationId.SkullCrypt, "Cripta de Calaveras", LocationKind.Dungeon, "Maps/SkullCrypt", true);
        Add(LocationId.VoidFortress, "Fortaleza del Vacío", LocationKind.Dungeon, "Maps/VoidFortress");
        Add(LocationId.BrokenMountains, "Montañas Quebradas", LocationKind.Outdoor, "Maps/Mountains");
        Add(LocationId.GiantsPass, "Paso de los Gigantes", LocationKind.Outdoor, "Maps/GiantsPass");
        Add(LocationId.SpectralForest, "Bosque Espectral", LocationKind.Outdoor, "Maps/SpectralForest");
        Add(LocationId.Docks, "Muelle de Wylgar", LocationKind.Outdoor, "Maps/Docks");
        Add(LocationId.SubmarineCaves, "Cuevas Submarinas", LocationKind.Dungeon, "Maps/SubmarineCaves", true);
        Add(LocationId.AshDesert, "Desierto de Cenizas", LocationKind.Outdoor, "Maps/AshDesert");
        Add(LocationId.EchoLake, "Lago del Eco", LocationKind.Outdoor, "Maps/EchoLake");

        TwoWay(LocationId.Farm, LocationId.BrokenBasement, new Point(30, 12), new Point(2, 2));
        TwoWay(LocationId.Farm, LocationId.Village, new Point(79, 20), new Point(1, 30));
        TwoWay(LocationId.Farm, LocationId.SpectralForest, new Point(0, 25), new Point(60, 18));
        TwoWay(LocationId.Village, LocationId.BrokenMountains, new Point(40, 0), new Point(25, 40),
            s => s.HasDungeonPermit, "Necesitas el permiso de calabozo de Lewthar.");
        TwoWay(LocationId.Village, LocationId.Docks, new Point(40, 49), new Point(20, 0));
        TwoWay(LocationId.Village, LocationId.Sewers, new Point(55, 30), new Point(5, 5),
            s => s.HasSewerKey, "La reja tiene un candado extraño. Krocus tendrá la llave.");
        TwoWay(LocationId.Village, LocationId.AshDesert, new Point(70, 40), new Point(3, 10),
            goldCost: 250);
        TwoWay(LocationId.BrokenMountains, LocationId.SoulMines, new Point(30, 5), new Point(10, 1),
            s => s.GuildRank >= 1, "Marlorn no te ha dado el rango de gremio necesario.");
        TwoWay(LocationId.BrokenMountains, LocationId.GiantsPass, new Point(55, 12), new Point(1, 10),
            s => s.GiantBridgeRepaired, "El puente está roto. Rurikka podría arreglarlo.");
        TwoWay(LocationId.BrokenMountains, LocationId.EchoLake, new Point(15, 15), new Point(10, 10));
        TwoWay(LocationId.Docks, LocationId.SubmarineCaves, new Point(35, 20), new Point(5, 3),
            s => s.LowTide, "La marea está alta. Vuelve cuando baje.");
        TwoWay(LocationId.SoulMines, LocationId.SkullCrypt, new Point(8, 8), new Point(2, 2),
            s => s.HighestBossFloorDefeated >= 35,
            "Un sello de almas bloquea la entrada. Derrota al jefe del Piso 35.");
        TwoWay(LocationId.SoulMines, LocationId.VoidFortress, new Point(20, 4), new Point(10, 40),
            s => s.GeneralsDefeated >= 7, "El Vacío te rechaza. Aún quedan Generales de Qixaroth.");

        Locations = locations;
        Warps = warps;

        void Add(LocationId id, string name, LocationKind kind, string tmx, bool procedural = false) =>
            locations.Add(id, new LocationDef
            {
                Id = id, Name = name, Kind = kind, TmxPath = tmx, IsProcedural = procedural
            });

        void TwoWay(LocationId a, LocationId b, Point tileA, Point tileB,
            Func<WorldState, bool>? open = null, string? locked = null, int goldCost = 0)
        {
            if (goldCost < 0)
                throw new ArgumentOutOfRangeException(nameof(goldCost));
            Func<WorldState, bool> condition = open ?? (_ => true);
            string message = locked ?? string.Empty;
            warps.Add(new Warp
            {
                From = a, To = b, FromTile = tileA, ToTile = tileB,
                IsOpen = condition, LockedMessage = message, GoldCost = goldCost
            });
            // El cobro se aplica al entrar al destino, no al regresar.
            warps.Add(new Warp
            {
                From = b, To = a, FromTile = tileB, ToTile = tileA,
                IsOpen = condition, LockedMessage = message
            });
        }
    }

    public static IEnumerable<Warp> WarpsFrom(LocationId id) => Warps.Where(warp => warp.From == id);
}

public sealed class WarpResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public LocationId Destination { get; init; }
    public Point SpawnTile { get; init; }
}

/// <summary>Controla la zona actual y procesa el uso de transiciones.</summary>
public sealed class LocationManager
{
    public LocationId Current { get; private set; } = LocationId.Farm;
    public WorldState State { get; } = new();
    public event Action<LocationId, LocationId>? LocationChanged;

    public WarpResult TryWarp(Point playerTile)
    {
        Warp? warp = WorldGraph.WarpsFrom(Current).FirstOrDefault(candidate => candidate.FromTile == playerTile);
        if (warp is null)
            return new WarpResult { Message = string.Empty };
        if (!warp.IsOpen(State))
            return new WarpResult { Message = warp.LockedMessage };
        if (State.Gold < warp.GoldCost)
            return new WarpResult { Message = $"Necesitas {warp.GoldCost} de oro." };

        State.Gold -= warp.GoldCost;
        LocationId from = Current;
        Current = warp.To;
        LocationChanged?.Invoke(from, Current);
        return new WarpResult { Success = true, Destination = warp.To, SpawnTile = warp.ToTile };
    }
}

/// <summary>Calendario simple; una semana lunar dura siete días de mundo.</summary>
public sealed class WorldCalendar
{
    public WorldCalendar(int startingDay = 1)
    {
        if (startingDay < 1)
            throw new ArgumentOutOfRangeException(nameof(startingDay));
        CurrentDay = startingDay;
    }

    public int CurrentDay { get; private set; }
    public int LunarWeek => (CurrentDay - 1) / 7 + 1;
    public int DayOfLunarWeek => (CurrentDay - 1) % 7 + 1;

    public void AdvanceDays(int days = 1)
    {
        if (days < 0)
            throw new ArgumentOutOfRangeException(nameof(days));
        CurrentDay = checked(CurrentDay + days);
    }
}

public enum Tile { Wall, Floor, Stairs, BossDoor, Spawn }

public sealed class DungeonFloor
{
    public int Floor { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public required Tile[,] Tiles { get; set; }
    public bool IsBossFloor { get; init; }
    public int Seed { get; init; }
}

/// <summary>Genera pisos roguelike deterministas para una semana lunar y piso.</summary>
public static class DungeonFloorGenerator
{
    public const int BossEvery = 7;

    public static int SeedFor(int lunarWeek, int floor)
    {
        unchecked
        {
            // Hash estable entre ejecuciones; HashCode.Combine usa una semilla aleatoria.
            uint hash = 2166136261;
            hash = (hash ^ (uint)lunarWeek) * 16777619;
            hash = (hash ^ (uint)floor) * 16777619;
            hash = (hash ^ 0x51A7u) * 16777619;
            return (int)hash;
        }
    }

    public static DungeonFloor Generate(int lunarWeek, int floor, int width = 48, int height = 32)
    {
        if (lunarWeek < 1)
            throw new ArgumentOutOfRangeException(nameof(lunarWeek));
        if (floor < 1)
            throw new ArgumentOutOfRangeException(nameof(floor));
        if (width < 16 || height < 16)
            throw new ArgumentOutOfRangeException(nameof(width), "El piso debe medir al menos 16 por 16.");

        int seed = SeedFor(lunarWeek, floor);
        var random = new Random(seed);
        bool boss = floor % BossEvery == 0;
        var result = new DungeonFloor
        {
            Floor = floor, Width = width, Height = height, Seed = seed,
            IsBossFloor = boss, Tiles = new Tile[width, height]
        };

        if (boss)
        {
            GenerateBossArena(result);
            return result;
        }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                result.Tiles[x, y] = x == 0 || y == 0 || x == width - 1 || y == height - 1 || random.NextDouble() < 0.45
                    ? Tile.Wall
                    : Tile.Floor;

        for (int i = 0; i < 4; i++) Smooth(result);

        var floors = new List<Point>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (result.Tiles[x, y] == Tile.Floor)
                    floors.Add(new Point(x, y));

        if (floors.Count < 2)
            return GenerateOpenFloor(lunarWeek, floor, width, height, seed);

        Point spawn = floors[random.Next(floors.Count)];
        Point stairs = floors.OrderByDescending(point => Math.Abs(point.X - spawn.X) + Math.Abs(point.Y - spawn.Y)).First();
        result.Tiles[spawn.X, spawn.Y] = Tile.Spawn;
        result.Tiles[stairs.X, stairs.Y] = Tile.Stairs;
        return result;
    }

    private static DungeonFloor GenerateOpenFloor(int lunarWeek, int floor, int width, int height, int seed)
    {
        var result = new DungeonFloor
        {
            Floor = floor, Width = width, Height = height, Seed = seed,
            IsBossFloor = false, Tiles = new Tile[width, height]
        };
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                result.Tiles[x, y] = x == 0 || y == 0 || x == width - 1 || y == height - 1
                    ? Tile.Wall : Tile.Floor;
        result.Tiles[1, 1] = Tile.Spawn;
        result.Tiles[width - 2, height - 2] = Tile.Stairs;
        return result;
    }

    private static void Smooth(DungeonFloor floor)
    {
        var next = (Tile[,])floor.Tiles.Clone();
        for (int x = 1; x < floor.Width - 1; x++)
            for (int y = 1; y < floor.Height - 1; y++)
            {
                int walls = 0;
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (floor.Tiles[x + dx, y + dy] == Tile.Wall)
                            walls++;
                next[x, y] = walls >= 5 ? Tile.Wall : Tile.Floor;
            }
        floor.Tiles = next;
    }

    private static void GenerateBossArena(DungeonFloor floor)
    {
        for (int x = 0; x < floor.Width; x++)
            for (int y = 0; y < floor.Height; y++)
            {
                bool edge = x < 8 || y < 6 || x >= floor.Width - 8 || y >= floor.Height - 6;
                floor.Tiles[x, y] = edge ? Tile.Wall : Tile.Floor;
            }
        floor.Tiles[floor.Width / 2, floor.Height - 7] = Tile.Spawn;
        floor.Tiles[floor.Width / 2, 6] = Tile.BossDoor;
    }
}
