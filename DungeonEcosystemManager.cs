using ValleQuebrado.Economy;

namespace ValleQuebrado.Dungeon;

/// <summary>Información de una criatura, su posición jerárquica y botín.</summary>
public sealed record MonsterData(
    string Id,
    string Name,
    string Rank,
    int HierarchyCount,
    string ZoneSource,
    string LoreDescription,
    IReadOnlyList<string> Drops);

/// <summary>Nodo mineral extraíble en los calabozos.</summary>
public sealed record MineralNode(string Id, string Name, int Difficulty, string RequiredTool);

public sealed record MineralMiningResult(bool Success, string Message, int QuantityAdded = 0);

/// <summary>Catálogo del ecosistema de calabozos y sus recursos minerales.</summary>
public static class DungeonEcosystemManager
{
    public const int SoldiersPerZone = 100;
    public const int CommandersPerZone = 7;
    public const int GeneralsPerZone = 1;

    private static readonly IReadOnlyList<MonsterData> MonsterDataList = Array.AsReadOnly(new[]
    {
        Monster("slime_sombra", "Slime de Sombra", "Soldado Raso", 100,
            "Sótano Roto / Pisos Generales",
            "Residuos gelatinosos de almas menores disueltas por la inestabilidad del Portal.",
            "Gelatina pegajosa", "Esencia de alma menor"),
        Monster("esqueleto_errante", "Esqueleto Errante", "Soldado Raso", 100,
            "Cripta de Calaveras",
            "Antiguos contratistas atrapados en deudas eternas del Consorcio Yoyo.",
            "Huesos rotos", "Polvo de hueso"),
        Monster("capataz_espectral", "Capataz de Esclavos Espectral", "Comandante Elite", 7,
            "Minas de Almas (Pisos Intermedios)",
            "Élites que vigilan las cuotas de extracción de almas en los túneles.",
            "Hierro de almas", "Plano rúnico"),
        Monster("malakor_juez", "Malakor, el Juez de Hierro", "General Supremo", 1,
            "Piso 7 (Sala del Trono)",
            "Antiguo juez del Gremio de Magos que vendió su conciencia para evadir la muerte.",
            "Trofeo de General", "Alma pura de combate"),
        Monster("vesperia_veda", "Vesperia, la Dama del Velo Espectral", "General Supremo", 1,
            "Piso 14 (Sala del Trono)",
            "Reina costera milenaria cuyo llanto congela el maná en las profundidades.",
            "Trofeo de General", "Alma pura de combate"),
        Monster("vulkarn_titan", "Vulkarn, el Titán de Magma Cautivo", "General Supremo", 1,
            "Piso 21 (Sala del Trono)",
            "Gigante de roca fundida a quien Qixaroth le arrebató el corazón.",
            "Trofeo de General", "Alma pura de combate"),
        Monster("qixaroth_trono", "Qixaroth, el Señor de las Almas Cautivas", "General Supremo", 1,
            "Fortaleza del Vacío (Piso Final)",
            "El Archimago original que busca un Imperio de Almas mediante el Consorcio Yoyo.",
            "Trofeo Supremo de Qixaroth", "Esencia del Trono")
    });

    private static readonly IReadOnlyList<MineralNode> MineralDataList = Array.AsReadOnly(new[]
    {
        new MineralNode("cobre_rocoso", "Cobre Rocoso", 1, "Pico de Cobre"),
        new MineralNode("hierro_espectral", "Hierro de Almas", 2, "Pico de Hierro"),
        new MineralNode("cristal_magma", "Cristal de Magma", 4, "Pico Encantado de Magma"),
        new MineralNode("obsidiana_vacia", "Obsidiana Abismal", 5, "Pico del Vacío")
    });

    private static readonly Dictionary<string, MonsterData> MonstersById =
        MonsterDataList.ToDictionary(monster => monster.Id, StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, MineralNode> MineralsById =
        MineralDataList.ToDictionary(mineral => mineral.Id, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<MonsterData> Monsters => MonsterDataList;
    public static IReadOnlyList<MineralNode> Minerals => MineralDataList;

    public static bool TryGetMonster(string id, out MonsterData? monster) => MonstersById.TryGetValue(id, out monster);

    public static IReadOnlyList<MonsterData> GetMonstersByRank(string rank) =>
        string.IsNullOrWhiteSpace(rank)
            ? Array.Empty<MonsterData>()
            : MonsterDataList.Where(monster => StringComparer.OrdinalIgnoreCase.Equals(monster.Rank, rank)).ToArray();

    public static bool TryGetMineral(string id, out MineralNode? mineral) => MineralsById.TryGetValue(id, out mineral);

    /// <summary>Valida la herramienta y la dificultad y agrega el mineral a los recursos del jugador.</summary>
    public static MineralMiningResult TryMine(
        string mineralId,
        string equippedTool,
        int miningSkill,
        ResourceManager resources,
        int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(resources);
        if (quantity < 1)
            return new MineralMiningResult(false, "La cantidad debe ser positiva.");
        if (!MineralsById.TryGetValue(mineralId, out MineralNode? mineral))
            return new MineralMiningResult(false, "El mineral no existe en el catálogo del calabozo.");
        if (miningSkill < mineral.Difficulty)
            return new MineralMiningResult(false, $"Necesitas nivel de minería {mineral.Difficulty}.");
        if (!StringComparer.OrdinalIgnoreCase.Equals(equippedTool, mineral.RequiredTool))
            return new MineralMiningResult(false, $"Necesitas {mineral.RequiredTool} para extraerlo.");

        resources.Gather(mineral.Id, quantity);
        return new MineralMiningResult(true, $"Recolectaste {quantity} de {mineral.Name}.", quantity);
    }

    private static MonsterData Monster(
        string id, string name, string rank, int count, string zone, string lore, params string[] drops) =>
        new(id, name, rank, count, zone, lore, Array.AsReadOnly(drops));
}
